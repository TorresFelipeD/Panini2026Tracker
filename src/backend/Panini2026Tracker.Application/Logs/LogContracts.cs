namespace Panini2026Tracker.Application.Logs;

public sealed record LogFilter(string? Category, string? Level, string? Search);

public sealed record SystemLogDto(Guid Id, string Category, string Action, string Details, string Level, DateTime CreatedAtUtc);

public sealed record DeleteLogsCommand(string? Category, string? Level, string? Search);

public interface ILogService
{
    Task<IReadOnlyCollection<SystemLogDto>> GetAllAsync(LogFilter filter, CancellationToken cancellationToken);
    Task<int> DeleteAsync(DeleteLogsCommand command, CancellationToken cancellationToken);
}
