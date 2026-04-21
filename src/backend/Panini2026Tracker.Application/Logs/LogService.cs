using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Application.Logs;

public sealed class LogService : ILogService
{
    private readonly ISystemLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogService(ISystemLogRepository logRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<SystemLogDto>> GetAllAsync(LogFilter filter, CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetAllAsync(cancellationToken);
        return logs
            .Where(log => string.IsNullOrWhiteSpace(filter.Category) || log.Category.Equals(filter.Category.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(log => string.IsNullOrWhiteSpace(filter.Level) || log.Level.Equals(filter.Level.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(log => string.IsNullOrWhiteSpace(filter.Search)
                || $"{log.Action} {log.Details} {log.Category}".Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(log => log.CreatedAtUtc)
            .Select(log => new SystemLogDto(log.Id, log.Category, log.Action, log.Details, log.Level, log.CreatedAtUtc))
            .ToArray();
    }

    public async Task<int> DeleteAsync(DeleteLogsCommand command, CancellationToken cancellationToken)
    {
        var deleted = await _logRepository.DeleteAsync(
            log =>
                (string.IsNullOrWhiteSpace(command.Category) || log.Category.Equals(command.Category.Trim(), StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(command.Level) || log.Level.Equals(command.Level.Trim(), StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrWhiteSpace(command.Search)
                    || $"{log.Action} {log.Details} {log.Category}".Contains(command.Search.Trim(), StringComparison.OrdinalIgnoreCase)),
            cancellationToken);

        await _logRepository.AddAsync(
            new SystemLog("logs", "logs.deleted", $"{deleted} log entries deleted with filters.", "warning", _dateTimeProvider.UtcNow),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return deleted;
    }
}
