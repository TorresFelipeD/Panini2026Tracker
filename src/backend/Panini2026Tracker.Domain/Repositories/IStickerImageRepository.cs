using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Domain.Repositories;

public interface IStickerImageRepository
{
    Task<StickerImage?> GetByStickerIdAsync(Guid stickerId, CancellationToken cancellationToken);
    Task AddAsync(StickerImage image, CancellationToken cancellationToken);
    void Remove(StickerImage image);
}
