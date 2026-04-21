using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Repositories;

public sealed class StickerDuplicateRepository : IStickerDuplicateRepository
{
    private readonly AppDbContext _dbContext;

    public StickerDuplicateRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StickerDuplicateEntry?> GetByStickerIdAsync(Guid stickerId, CancellationToken cancellationToken) =>
        _dbContext.StickerDuplicateEntries.FirstOrDefaultAsync(entry => entry.StickerCatalogItemId == stickerId, cancellationToken);

    public Task AddAsync(StickerDuplicateEntry entry, CancellationToken cancellationToken) =>
        _dbContext.StickerDuplicateEntries.AddAsync(entry, cancellationToken).AsTask();

    public void Remove(StickerDuplicateEntry entry) => _dbContext.StickerDuplicateEntries.Remove(entry);
}
