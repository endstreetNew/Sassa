using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sassa.Services
{
    public class ScheduleService : BackgroundService
    {
        private readonly ILogger<ScheduleService> _logger;
        private readonly SocpenService _socpen;
        IOptions<ScheduleOptions> _scheduleOptions;
        private Timer? _timer;

        public ScheduleService(ILogger<ScheduleService> logger, IOptions<ScheduleOptions> options)
        {
            _logger = logger;
            _scheduleOptions = options;
            _socpen = new SocpenService(options.Value.ConnectionString);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            ScheduleNextRun();
            return Task.CompletedTask;
        }

        private void RunScheduledTask(object? state)
        {
            
            if (_scheduleOptions.Value.Enabled)
            {
                _logger.LogInformation("Executing SyncSocpen() task at: {Time}", DateTime.Now);
                _socpen.SyncSocpen();
            }
            else
            {
                _logger.LogInformation("SyncSocpen() Task is disabled and not run.", DateTime.Now);
            }
            ScheduleNextRun();
        }
        private void ScheduleNextRun()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(_scheduleOptions.Value.RunAtHour); // 04:00 today
            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1); // 04:00 next day if already past
            }
            var timeToGo = nextRun - now;
            _logger.LogInformation($"Socpen Task Scheduled for {nextRun.ToShortTimeString()}");
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Socpen Task Scheduler stopping...");
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(stoppingToken);
        }

    }
}