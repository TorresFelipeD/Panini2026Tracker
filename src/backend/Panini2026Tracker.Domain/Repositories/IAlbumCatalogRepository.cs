using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Domain.Repositories;

public interface IAlbumCatalogRepository
{
    Task<IReadOnlyCollection<Country>> GetCountriesWithStickersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StickerCatalogItem>> GetStickersWithRelationsAsync(CancellationToken cancellationToken);
    Task<StickerCatalogItem?> GetStickerByIdAsync(Guid stickerId, CancellationToken cancellationToken);
    Task<bool> HasSeedDataAsync(CancellationToken cancellationToken);
    Task AddCountriesAsync(IEnumerable<Country> countries, CancellationToken cancellationToken);
    Task AddStickersAsync(IEnumerable<StickerCatalogItem> stickers, CancellationToken cancellationToken);
}
