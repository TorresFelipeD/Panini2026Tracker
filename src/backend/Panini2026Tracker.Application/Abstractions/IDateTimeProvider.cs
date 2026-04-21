namespace Panini2026Tracker.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
