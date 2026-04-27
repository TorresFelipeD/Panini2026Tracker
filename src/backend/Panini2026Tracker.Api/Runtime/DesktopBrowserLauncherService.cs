using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Panini2026Tracker.Api.Runtime;

public sealed class DesktopBrowserLauncherService : IHostedService
{
    private readonly DesktopRuntimeOptions _options;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServer _server;
    private readonly ILogger<DesktopBrowserLauncherService> _logger;

    public DesktopBrowserLauncherService(
        DesktopRuntimeOptions options,
        IHostApplicationLifetime applicationLifetime,
        IServer server,
        ILogger<DesktopBrowserLauncherService> logger)
    {
        _options = options;
        _applicationLifetime = applicationLifetime;
        _server = server;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Enabled)
        {
            _applicationLifetime.ApplicationStarted.Register(OpenBrowser);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OpenBrowser()
    {
        var address = _server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.LogWarning("Desktop runtime could not determine the listening address.");
            return;
        }

        var launchUrl = $"{address.TrimEnd('/')}/?desktopSession={_options.SessionToken}";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = launchUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Desktop runtime could not open the browser automatically.");
        }
    }
}
