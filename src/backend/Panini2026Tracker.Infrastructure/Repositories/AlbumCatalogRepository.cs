using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Repositories;

public sealed class AlbumCatalogRepository : IAlbumCatalogRepository
{
    private readonly AppDbContext _dbContext;

    public AlbumCatalogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Country>> GetCountriesWithStickersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Countries
            .Include(country => country.Stickers)
                .ThenInclude(sticker => sticker.CollectionEntry)
            .Include(country => country.Stickers)
                .ThenInclude(sticker => sticker.DuplicateEntry)
            .Include(country => country.Stickers)
                .ThenInclude(sticker => sticker.StickerImage)
            .OrderBy(country => country.DisplayOrder)
            .ThenBy(country => country.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<StickerCatalogItem?> GetStickerByIdAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        return _dbContext.StickerCatalogItems
            .Include(sticker => sticker.Country)
            .Include(sticker => sticker.CollectionEntry)
            .Include(sticker => sticker.DuplicateEntry)
            .Include(sticker => sticker.StickerImage)
            .FirstOrDefaultAsync(sticker => sticker.Id == stickerId, cancellationToken);
    }

    public Task<bool> HasSeedDataAsync(CancellationToken cancellationToken) =>
        _dbContext.StickerCatalogItems.AnyAsync(cancellationToken);

    public async Task AddCountriesAsync(IEnumerable<Country> countries, CancellationToken cancellationToken) =>
        await _dbContext.Countries.AddRangeAsync(countries, cancellationToken);

    public async Task AddStickersAsync(IEnumerable<StickerCatalogItem> stickers, CancellationToken cancellationToken) =>
        await _dbContext.StickerCatalogItems.AddRangeAsync(stickers, cancellationToken);
}
