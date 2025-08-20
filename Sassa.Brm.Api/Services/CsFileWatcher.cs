using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sassa.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sassa.BRM.Services
{
    public class CsFileWatcher : BackgroundService
    {
        private readonly ILogger<CsFileWatcher> _logger;
        private readonly string _watchDirectory;
        private Timer? _timer;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(120);
        CsUploadService _csService;

        public CsFileWatcher(
            ILogger<CsFileWatcher> logger,IConfiguration _config, CsUploadService csService)
        {
            _csService = csService;
            _logger = logger;
            _watchDirectory = _config.GetValue<string>($"Urls:ScanFolderRoot")! + "\\Processed"; 
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(_watchDirectory))
                Directory.CreateDirectory(_watchDirectory);

            _timer = new Timer(ProcessFiles, null, TimeSpan.Zero, _pollInterval);

            return Task.CompletedTask;
        }

        private async void ProcessFiles(object? state)
        {
            try
            {
                var files = Directory.GetFiles(_watchDirectory, "*.pdf");
                if (files.Length == 0)
                {
                    // No files, stop timer until next interval
                    return;
                }

                foreach (var file in files)
                {
                    // Wait for file to be ready
                    for (int i = 0; i< 100; i++)
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
                            await Task.Delay(500);
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
                _logger.LogError(ex, "Error processing files in directory {Dir}", _watchDirectory);
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
