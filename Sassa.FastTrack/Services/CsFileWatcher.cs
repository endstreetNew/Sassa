using Sassa.Services;

namespace Sassa.BRM.Services
{
    public class CsFileWatcher : BackgroundService
    {
        private readonly ILogger<CsFileWatcher> _logger;
        private readonly string _processedDirectory;
        private System.Threading.Timer? _timer;
        private readonly TimeSpan _pollInterval;
        CsUploadService _csService;
        // Re-entrancy guard (0 = idle, 1 = running)
        private int _isProcessing;
        private CancellationToken _stoppingToken;
        // Pause flag (0 = running, 1 = paused)
        private int _isPaused;

        public CsFileWatcher(
            ILogger<CsFileWatcher> logger, IConfiguration _config, CsUploadService csService)
        {
            _csService = csService;
            _logger = logger;
            _processedDirectory = Path.Combine(_config.GetValue<string>($"Urls:ScanFolderRoot")!, "Processed");
            _pollInterval = TimeSpan.FromSeconds(_config.GetValue<int>("Functions:CsPollIntervalSeconds"));
            _isPaused = _config.GetValue<bool>($"Functions:CsFileWatcher") ? 0 : 1;
        }

        // Public pause that can be called from any thread
        public void Pause()
        {
            Interlocked.Exchange(ref _isPaused, 1);
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _logger.LogInformation("CsFileWatcher paused.");
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
            _logger.LogInformation("CsFileWatcher resumed.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            if (!Directory.Exists(_processedDirectory)) Directory.CreateDirectory(_processedDirectory);

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
                    files = Directory.GetFiles(_processedDirectory, "*.pdf");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enumerate files in {Dir}", _processedDirectory);
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
                    var csNode = fileName.Substring(0, 13);

                    try
                    {
                        await UploadToContentserver(csNode, file);
                        File.Delete(file);
                        _logger.LogInformation("Uploaded and deleted file {File}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading file {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files in directory {Dir}", _processedDirectory);
            }
            finally
            {
                // Resume timer only if not stopping and not paused
                if (!_stoppingToken.IsCancellationRequested && Volatile.Read(ref _isPaused) == 0)
                {
                    _timer?.Change(_pollInterval, _pollInterval);
                    _logger.LogInformation("Cs Files: I am alive..!");
                }
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

        private async Task UploadToContentserver(string csNode, string file)
        {
            // Retry strategy for UploadDoc
            const int maxRetries = 3;
            const int delayMs = 10000;
            int attempt = 0;
            Exception? lastException = null;
            while (attempt < maxRetries)
            {
                try
                {
                    await _csService.UploadGrantDoc(csNode, file);
                    lastException = null;
                    break; // Success
                }
                catch (Exception ex)
                {
                    if (ex.Message.EndsWith("exists."))
                    {
                        File.Delete(file);
                        break;
                    }
                    lastException = ex;
                    attempt++;
                    if (attempt < maxRetries)
                        await Task.Delay(delayMs);
                }
            }
            if (lastException != null)
                throw new Exception($"Upload to CS failed after {maxRetries} attempts. (Retry)");

        }

    }

}
