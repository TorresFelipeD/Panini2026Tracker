using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Domain.Repositories;

public interface IStickerCollectionRepository
{
    Task<StickerCollectionEntry?> GetByStickerIdAsync(Guid stickerId, CancellationToken cancellationToken);
    Task AddAsync(StickerCollectionEntry entry, CancellationToken cancellationToken);
}
