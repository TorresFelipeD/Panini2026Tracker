using System.Linq.Expressions;
using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Domain.Repositories;

public interface ISystemLogRepository
{
    Task<IReadOnlyCollection<SystemLog>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(SystemLog log, CancellationToken cancellationToken);
    Task<int> DeleteAsync(Expression<Func<SystemLog, bool>> predicate, CancellationToken cancellationToken);
}
