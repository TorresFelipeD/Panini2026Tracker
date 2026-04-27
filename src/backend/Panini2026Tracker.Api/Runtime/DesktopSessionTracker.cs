namespace Panini2026Tracker.Api.Runtime;

public sealed class DesktopSessionTracker
{
    private readonly DesktopRuntimeOptions _options;
    private readonly object _syncRoot = new();
    private DateTimeOffset? _lastHeartbeatUtc;
    private bool _hasConnected;
    private bool _isClosed;

    public DesktopSessionTracker(DesktopRuntimeOptions options)
    {
        _options = options;
    }

    public bool TryRecordHeartbeat(string? token)
    {
        if (!IsValidToken(token))
        {
            return false;
        }

        lock (_syncRoot)
        {
            _hasConnected = true;
            _isClosed = false;
            _lastHeartbeatUtc = DateTimeOffset.UtcNow;
        }

        return true;
    }

    public bool TryClose(string? token)
    {
        if (!IsValidToken(token))
        {
            return false;
        }

        lock (_syncRoot)
        {
            _hasConnected = true;
            _isClosed = true;
            _lastHeartbeatUtc = DateTimeOffset.UtcNow;
        }

        return true;
    }

    public DesktopSessionSnapshot GetSnapshot()
    {
        lock (_syncRoot)
        {
            return new DesktopSessionSnapshot(
                _options.Enabled,
                _options.StartedAtUtc,
                _options.HeartbeatTimeout,
                _options.StartupTimeout,
                _hasConnected,
                _isClosed,
                _lastHeartbeatUtc);
        }
    }

    private bool IsValidToken(string? token)
    {
        return _options.Enabled
            && !string.IsNullOrWhiteSpace(token)
            && string.Equals(token, _options.SessionToken, StringComparison.Ordinal);
    }
}

public sealed record DesktopSessionSnapshot(
    bool Enabled,
    DateTimeOffset StartedAtUtc,
    TimeSpan HeartbeatTimeout,
    TimeSpan StartupTimeout,
    bool HasConnected,
    bool IsClosed,
    DateTimeOffset? LastHeartbeatUtc);
