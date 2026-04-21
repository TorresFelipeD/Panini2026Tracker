using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Domain.Repositories;

public interface IStickerDuplicateRepository
{
    Task<StickerDuplicateEntry?> GetByStickerIdAsync(Guid stickerId, CancellationToken cancellationToken);
    Task AddAsync(StickerDuplicateEntry entry, CancellationToken cancellationToken);
    void Remove(StickerDuplicateEntry entry);
}
