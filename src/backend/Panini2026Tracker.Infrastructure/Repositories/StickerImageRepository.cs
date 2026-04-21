using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Repositories;

public sealed class StickerImageRepository : IStickerImageRepository
{
    private readonly AppDbContext _dbContext;

    public StickerImageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<StickerImage?> GetByStickerIdAsync(Guid stickerId, CancellationToken cancellationToken) =>
        _dbContext.StickerImages.FirstOrDefaultAsync(image => image.StickerCatalogItemId == stickerId, cancellationToken);

    public Task AddAsync(StickerImage image, CancellationToken cancellationToken) =>
        _dbContext.StickerImages.AddAsync(image, cancellationToken).AsTask();

    public void Remove(StickerImage image) =>
        _dbContext.StickerImages.Remove(image);
}
