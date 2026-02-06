using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BotFlow.Application.Interfaces;

namespace BotFlow.Application.Services
{
    public class StatisticsBackgroundService : IHostedService, IDisposable
    {
        private readonly ILogger<StatisticsBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public StatisticsBackgroundService(
            ILogger<StatisticsBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Statistics Background Service is starting.");

            // Start the timer to run every hour
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var kpiService = scope.ServiceProvider.GetRequiredService<IKPIService>();
                    await kpiService.UpdateSystemStatisticsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating statistics in background service");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Statistics Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}