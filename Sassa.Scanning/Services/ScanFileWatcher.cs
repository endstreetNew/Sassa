

using iText.Kernel.Pdf;
using Sassa.Scanning.Models;
using Sassa.Scanning.Settings;
using System.Windows.Forms;

namespace Sassa.BRM.Services
{
    public class ScanFileWatcher : BackgroundService
    {
        private readonly ILogger<ScanFileWatcher> _logger;
        private System.Threading.Timer? _timer;
        private readonly TimeSpan _pollInterval;
        // Re-entrancy guard (0 = idle, 1 = running)
        private int _isProcessing;
        private CancellationToken _stoppingToken;
        // Pause flag (0 = running, 1 = paused)
        private int _isPaused;
        ScanningSettings _settings;
        PdfBarcodeSplitter _splitter;

        public ScanFileWatcher(
            ILogger<ScanFileWatcher> logger, IConfiguration _config,ScanningSettings settings,PdfBarcodeSplitter splitter)
        {
            _logger = logger;
            _pollInterval = TimeSpan.FromSeconds(_config.GetValue<int>("Functions:ScanPollIntervalSeconds"));
            _isPaused = _config.GetValue<bool>($"Functions:ScanFileWatcher") ? 0 : 1;
            _settings = settings;
            _splitter = splitter;
        }

        // Public pause that can be called from any thread
        public void Pause()
        {
            Interlocked.Exchange(ref _isPaused, 1);
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _logger.LogInformation("ScanFileWatcher paused.");
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
            _logger.LogInformation("ScanFileWatcher resumed.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            if (!Directory.Exists(_settings.PdfPath))
            {
                return Task.CompletedTask;
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
                    files = Directory.GetFiles(_settings.PdfPath, "*.pdf",SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enumerate files in {Dir}", _settings.PdfPath);
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

                    //var fileName = Path.GetFileName(file);
                    

                    try
                    {
                        //Get the source document
                        PdfScanDocument source = ReadSource(file);

                        _splitter.SplitPdfByTwoBarcodes(source, _settings.OutputPath, _settings);

                        File.Move(file, Path.Combine(_settings.ScannedPath, Path.GetFileName(file)), true);
                        _logger.LogInformation("Processed file {File}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading file {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files in directory {Dir}", _settings.PdfPath);
            }
            finally
            {
                // Resume timer only if not stopping and not paused
                if (!_stoppingToken.IsCancellationRequested && Volatile.Read(ref _isPaused) == 0)
                {
                    _timer?.Change(_pollInterval, _pollInterval);
                    _logger.LogInformation("Scan Files: I am alive..!");
                }
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

        private PdfScanDocument SplitPdfToPages(byte[] sourcePdfBytes)
        {
            var result = new PdfScanDocument();
            result.Content = sourcePdfBytes;
            using (var srcStream = new MemoryStream(sourcePdfBytes))
            using (var pdfDoc = new PdfDocument(new PdfReader(srcStream)))
            {
                result.endPage = pdfDoc.GetNumberOfPages();

                for (int pageNum = 1; pageNum <= result.endPage; pageNum++)
                {
                    PdfScanPage page = new PdfScanPage() { PageNumber = pageNum };
                    using (var outStream = new MemoryStream())
                    {
                        using (var writer = new PdfWriter(outStream))
                        using (var newDoc = new PdfDocument(writer))
                        {
                            pdfDoc.CopyPagesTo(pageNum, pageNum, newDoc);
                            page.ps = newDoc.GetFirstPage().GetPageSize();
                        }
                        page.Content = outStream.ToArray();
                        page.PageNumber = pageNum;
                    }
                    result.Pages.Add(page);
                }
            }

            return result;
        }

        private PdfScanDocument ReadSource(string filePath)
        {
            byte[] sourcePdfBytes = File.ReadAllBytes(filePath);

            // Split into individual page PDFs in memory
            return SplitPdfToPages(sourcePdfBytes);
        }
    }

}
