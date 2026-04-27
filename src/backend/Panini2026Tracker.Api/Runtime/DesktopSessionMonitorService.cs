using Microsoft.Extensions.Hosting;

namespace Panini2026Tracker.Api.Runtime;

public sealed class DesktopSessionMonitorService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private readonly DesktopSessionTracker _tracker;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<DesktopSessionMonitorService> _logger;

    public DesktopSessionMonitorService(
        DesktopSessionTracker tracker,
        IHostApplicationLifetime applicationLifetime,
        ILogger<DesktopSessionMonitorService> logger)
    {
        _tracker = tracker;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshot = _tracker.GetSnapshot();
            if (ShouldStop(snapshot, out var reason))
            {
                _logger.LogInformation("Desktop runtime shutdown triggered: {Reason}", reason);
                _applicationLifetime.StopApplication();
                return;
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private static bool ShouldStop(DesktopSessionSnapshot snapshot, out string reason)
    {
        reason = string.Empty;

        if (!snapshot.Enabled)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;

        if (!snapshot.HasConnected && now - snapshot.StartedAtUtc >= snapshot.StartupTimeout)
        {
            reason = "Browser session did not connect in time.";
            return true;
        }

        if (snapshot.IsClosed)
        {
            reason = "Browser session closed.";
            return true;
        }

        if (snapshot.HasConnected
            && snapshot.LastHeartbeatUtc is { } lastHeartbeatUtc
            && now - lastHeartbeatUtc >= snapshot.HeartbeatTimeout)
        {
            reason = "Browser heartbeat expired.";
            return true;
        }

        return false;
    }
}
