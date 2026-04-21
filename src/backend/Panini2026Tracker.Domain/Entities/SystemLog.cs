using Panini2026Tracker.Domain.Common;

namespace Panini2026Tracker.Domain.Entities;

public sealed class SystemLog : BaseEntity
{
    public string Category { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private SystemLog()
    {
    }

    public SystemLog(string category, string action, string details, string level, DateTime createdAtUtc)
    {
        Category = category.Trim();
        Action = action.Trim();
        Details = details.Trim();
        Level = level.Trim().ToLowerInvariant();
        CreatedAtUtc = createdAtUtc;
    }
}
