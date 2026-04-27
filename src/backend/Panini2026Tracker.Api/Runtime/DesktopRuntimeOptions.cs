using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;

namespace Panini2026Tracker.Api.Runtime;

public sealed record DesktopRuntimeOptions(
    bool Enabled,
    string BaseAddress,
    string SessionToken,
    DateTimeOffset StartedAtUtc,
    TimeSpan HeartbeatTimeout,
    TimeSpan StartupTimeout)
{
    public static DesktopRuntimeOptions Create(IHostEnvironment environment)
    {
        var webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
        var hasPublishedFrontend = File.Exists(Path.Combine(webRootPath, "index.html"));

        if (environment.IsDevelopment() || !hasPublishedFrontend)
        {
            return new DesktopRuntimeOptions(
                false,
                string.Empty,
                string.Empty,
                DateTimeOffset.UtcNow,
                TimeSpan.Zero,
                TimeSpan.Zero);
        }

        var port = GetAvailablePort();

        return new DesktopRuntimeOptions(
            true,
            $"http://127.0.0.1:{port}",
            Guid.NewGuid().ToString("N"),
            DateTimeOffset.UtcNow,
            TimeSpan.FromSeconds(12),
            TimeSpan.FromSeconds(45));
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
