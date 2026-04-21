using Panini2026Tracker.Application.Abstractions;

namespace Panini2026Tracker.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
