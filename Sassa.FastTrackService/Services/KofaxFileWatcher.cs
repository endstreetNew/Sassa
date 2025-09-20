using iText.StyledXmlParser.Jsoup.Helper;
using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.Models;
using Sassa.Services;

namespace Sassa.Services
{
    public class KofaxFileWatcher : BackgroundService
    {
        private readonly ILogger<KofaxFileWatcher> _logger;
        private readonly string _watchDirectory;
        private readonly string _processedDirectory;
        private readonly string _rejectDirectory;
        private readonly string _repairDirectory;
        private System.Threading.Timer? _timer;
        private readonly TimeSpan _pollInterval;
        private readonly LoService _loService;
        private readonly ValidationService _validation;
        // Re-entrancy guard (0 = idle, 1 = running)
        private int _isProcessing;
        private CancellationToken _stoppingToken;
        // Pause flag (0 = running, 1 = paused)
        private int _isPaused;

        public KofaxFileWatcher(ILogger<KofaxFileWatcher> logger,FastTrackServiceSettings settings, LoService loService, ValidationService validation)
        {
            _logger = logger;
            _watchDirectory = settings.Urls.ScanFolderRoot;
            _processedDirectory = Path.Combine(_watchDirectory, "Processed");
            _rejectDirectory = Path.Combine(_watchDirectory, "Rejected");
            _repairDirectory = Path.Combine(_watchDirectory, "RepairQueue");
            _pollInterval = TimeSpan.FromSeconds(settings.Functions.KofaxPollIntervalSeconds);
            _loService = loService;
            _validation = validation;
        }

        // Public pause that can be called from any thread
        public void Pause()
        {
            Interlocked.Exchange(ref _isPaused, 1);
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _logger.LogInformation("KofaxFileWatcher paused.");
        }

        // Public resume that can be called from any thread
        public void Resume()
        {
            // If we were paused, clear the flag and restart the timer
            var wasPaused = Interlocked.Exchange(ref _isPaused, 0);
            if (wasPaused == 0) return; // already running
            if (_stoppingToken.IsCancellationRequested) return;

            if (_timer != null)
            {
                _timer.Change(TimeSpan.Zero, _pollInterval);
            }
            _logger.LogInformation("KofaxFileWatcher resumed.");
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            try
            {
                if (!Directory.Exists(_watchDirectory)) Directory.CreateDirectory(_watchDirectory);
                if (!Directory.Exists(_processedDirectory)) Directory.CreateDirectory(_processedDirectory);
                if (!Directory.Exists(_rejectDirectory)) Directory.CreateDirectory(_rejectDirectory);
                if (!Directory.Exists(_repairDirectory)) Directory.CreateDirectory(_repairDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directories");
                throw;
            }

            var dueTime = Volatile.Read(ref _isPaused) == 1 ? Timeout.InfiniteTimeSpan : TimeSpan.Zero;
            _timer = new System.Threading.Timer(ProcessFiles, null, dueTime, _pollInterval);

            return Task.CompletedTask;
        }

        private async void ProcessFiles(object? state)
        {
            // If paused, do nothing
            if (Volatile.Read(ref _isPaused) == 1)
            {
                return;
            }
            // Prevent overlapping executions
            if (Interlocked.Exchange(ref _isProcessing, 1) == 1)
            {
                // Another run is still active
                return;
            }

            // Pause timer while we work (avoid piling callbacks)
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            try
            {
                if (_stoppingToken.IsCancellationRequested) return;
                string[] files;
                try
                {
                    files = Directory.GetFiles(_watchDirectory, "*.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enumerate files in {Dir}", _watchDirectory);
                    return;
                }
                if (files.Length == 0)
                {
                    // No files, stop timer until next interval
                    return;
                }

                foreach (var file in files)
                {
                    // Wait for file to be ready
                    for (int i = 0; i < 100; i++)
                    {
                        try
                        {
                            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            await Task.Delay(1000);
                        }
                    }

                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var fileParts = fileName.Split('.');
                    fileName = fileName + ".pdf";
                    FasttrackScan scanModel = new FasttrackScan
                    {
                        LoReferece = fileParts[0],
                        BrmBarcode = fileParts[1].ToUpper(),
                    };

                    if (scanModel.LoReferece.Length != 16 || scanModel.BrmBarcode.Length != 8 || !scanModel.LoReferece.IsNumeric())
                    {
                        _logger.LogError("Invalid file name format: {FileName}. Expected format: <16-digit LO Reference>.<8-digit BRM Barcode>", fileName);
                        if (!File.Exists(Path.Combine(_rejectDirectory, "InvalidFileName." + fileName)))
                        {
                            File.Move(file, Path.Combine(_rejectDirectory, "InvalidFileName." + fileName));
                        }
                        else
                        {
                            File.Delete(file);
                        }
                        continue;

                    }
                    //Check for dupliate Barcode
                    if (_validation.BrmRecordExists(scanModel.BrmBarcode))
                    {
                        _logger.LogError("Duplicate BRM Barcode", fileName);
                        if (!File.Exists(Path.Combine(_rejectDirectory, "DuplicateBarcode." + fileName)))
                        {
                            await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now, Validationresult = "Duplicate Barcode(ok)" });
                            File.Move(file, Path.Combine(_rejectDirectory, "DuplicateBarcode." + fileName));
                        }
                        else
                        {
                            File.Delete(file);
                        }
                        continue;
                    }
                    //---------------------------------
                    CustCoversheet? coverSheet = await _loService.GetCoversheetAsync(scanModel.LoReferece);
                    if (coverSheet is null)
                    {
                        _logger.LogError("LOReferenceNotFound", fileName);
                        if (!File.Exists(Path.Combine(_rejectDirectory, "LOReferencNotFound." + fileName)))
                        {
                            await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now, Validationresult = "LO Reference not found." });
                            File.Move(file, Path.Combine(_rejectDirectory, "LOReferencNotFound." + fileName));
                        }
                        else
                        {
                            File.Delete(file);
                         }
                        continue;
                    }
                    try
                    {
                        DcFile dcFile = await _validation.GetDcFileFromLoAsync(coverSheet!, scanModel, file, fileName);
                        string result = _validation.Validate(dcFile);
                        if (!string.IsNullOrEmpty(result)) throw new Exception(result);
                        dcFile = await _validation.CheckForSocpenRecordAsync(dcFile, scanModel.LoReferece);
                        //File is valid and ready to be processed
                        //Rename the file
                        var newFileName = _validation.GetFilename(dcFile);
                        string newFilePath = $@"{_processedDirectory}\" + newFileName;
                        //Just move the file
                        if (!File.Exists(newFilePath))
                        {
                            File.Move(file, newFilePath);
                        }
                        else
                        {
                            _logger.LogInformation("File {newFilePath} already exists in processed folder, Deleting duplicate", newFilePath);
                            File.Delete(file);
                        }
                        _logger.LogInformation("Processed file {File}", file);
                        await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now, Validationresult = "ok" });
                    }
                    catch (Exception ex)
                    {
                        
                        await _loService.UpdateValidation(new CustCoversheetValidation { ReferenceNum = scanModel.LoReferece, ValidationDate = DateTime.Now, Validationresult = ex.Message });
                        _logger.LogInformation("Repair Queue file {File}", file);
                        if (!File.Exists(Path.Combine(_repairDirectory, fileName)))
                        {
                            File.Move(file, Path.Combine(_repairDirectory, fileName));
                        }
                        else
                        {
                            File.Delete(file);
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error processing files in directory {Dir}", _watchDirectory);
            }
            finally
            {
                // Resume timer only if not stopping
                if (!_stoppingToken.IsCancellationRequested)
                {
                    _timer?.Change(_pollInterval, _pollInterval);
                    _logger.LogInformation("Kofax Files: I am alive..!");
                }
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

    }
}
