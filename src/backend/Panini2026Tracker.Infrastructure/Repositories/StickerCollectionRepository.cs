using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Repositories;

public sealed class StickerCollectionRepository : IStickerCollectionRepository
{
    private readonly AppDbContext _dbContext;

    public StickerCollectionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StickerCollectionEntry?> GetByStickerIdAsync(Guid stickerId, CancellationToken cancellationToken) =>
        _dbContext.StickerCollectionEntries.FirstOrDefaultAsync(entry => entry.StickerCatalogItemId == stickerId, cancellationToken);

    public Task AddAsync(StickerCollectionEntry entry, CancellationToken cancellationToken) =>
        _dbContext.StickerCollectionEntries.AddAsync(entry, cancellationToken).AsTask();
}
