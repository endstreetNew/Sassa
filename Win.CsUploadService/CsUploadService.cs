using System;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Configuration;
using Serilog;

using Sassa.CsService;


namespace Win.CsUploadService
{
    public partial class CsUploadService : ServiceBase
    {
        private ILogger _logger;
        private FileSystemWatcher _watcher;
        private string _watchDirectory;
        //private CSService _csService;

        public CsUploadService()
        {
            InitializeComponent();
            // You can use App.config or hardcode the path
            _watchDirectory = ConfigurationManager.AppSettings["WatchDirectory"]
                              ?? @"C:\CsUpload\Incoming";

            // Ensure logs directory exists
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            // Configure Serilog
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(logDir, "service.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    fileSizeLimitBytes: 10_000_000,
                    shared: true)
                .CreateLogger();
        }

        protected override void OnStart(string[] args)
        {
            if (!Directory.Exists(_watchDirectory))
                Directory.CreateDirectory(_watchDirectory);

            _watcher = new FileSystemWatcher(_watchDirectory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.*", // Watch all files
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            _watcher.Created += OnFileCreated;
            _watcher.Error += OnWatcherError;
            _logger.Information("Service started at {Time}", DateTime.Now);
        }

        protected override void OnStop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileCreated;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
            }
            _logger.Information("Service stopped at {Time}", DateTime.Now);
            Log.CloseAndFlush();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            // Wait for the file to be fully written
            Task.Run(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        using (var stream = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            // File is ready
                            break;
                        }
                    }
                    catch (IOException)
                    {
                        await Task.Delay(500);
                    }
                }

                ProcessFile(e.FullPath);
            });
        }
        // Example usage in your file processing logic
        private void ProcessFile(string filePath)
        {
            try
            {
                // TODO: Add your file processing logic here
                // Example: Move to processed folder
                string processedDir = Path.Combine(_watchDirectory, "Processed");
                if (!Directory.Exists(processedDir))
                    Directory.CreateDirectory(processedDir);

                string destPath = Path.Combine(processedDir, Path.GetFileName(filePath));
                File.Move(filePath, destPath);
                // Your file processing code here
                _logger.Information("Processing file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing file: {FilePath}", filePath);
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _logger.Error(e.GetException().ToString());
        }

        private async Task UploadToContentserver(string csNode, string file)
        {
            // Retry strategy for UploadDoc
            const int maxRetries = 3;
            const int delayMs = 2000;
            int attempt = 0;
            Exception? lastException = null;
            while (attempt < maxRetries)
            {
                try
                {
                    await csService.UploadGrantDoc(csNode, file);
                    lastException = null;
                    break; // Success
                }
                catch (Exception ex)
                {
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
