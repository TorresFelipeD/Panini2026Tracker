using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Repositories;

public sealed class SystemLogRepository : ISystemLogRepository
{
    private readonly AppDbContext _dbContext;

    public SystemLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<SystemLog>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.SystemLogs.AsNoTracking().ToArrayAsync(cancellationToken);

    public Task AddAsync(SystemLog log, CancellationToken cancellationToken) =>
        _dbContext.SystemLogs.AddAsync(log, cancellationToken).AsTask();

    public Task<int> DeleteAsync(Func<SystemLog, bool> predicate, CancellationToken cancellationToken)
    {
        var logs = _dbContext.SystemLogs.Where(log => predicate(log)).ToArray();
        _dbContext.SystemLogs.RemoveRange(logs);
        return Task.FromResult(logs.Length);
    }
}
