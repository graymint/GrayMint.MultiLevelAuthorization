using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MultiLevelAuthorization.Server;

public class TimedHostedService : IHostedService, IDisposable
{
    private readonly ILogger<TimedHostedService> _logger;
    private Timer? _timer;
    private readonly TimeSpan _timerInterval = TimeSpan.FromMinutes(120);

    public TimedHostedService(ILogger<TimedHostedService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(state => _ = DoWork(), null, _timerInterval, Timeout.InfiniteTimeSpan);
        return Task.CompletedTask;
    }

    private async Task DoWork()
    {
        try
        {
            _timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            await Task.Delay(0);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Cleanup error. Error: {ex}");
        }
        finally
        {
            _timer?.Change(_timerInterval, Timeout.InfiniteTimeSpan);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(TimedHostedService)} is stopping.");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}