using Panini2026Tracker.Domain.Common;

namespace Panini2026Tracker.Domain.Entities;

public sealed class StickerDuplicateEntry : BaseEntity
{
    public Guid StickerCatalogItemId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public StickerCatalogItem StickerCatalogItem { get; private set; } = default!;

    private StickerDuplicateEntry()
    {
    }

    public StickerDuplicateEntry(Guid stickerCatalogItemId, int quantity, DateTime updatedAtUtc)
    {
        StickerCatalogItemId = stickerCatalogItemId;
        Quantity = Math.Max(0, quantity);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void UpdateQuantity(int quantity, DateTime updatedAtUtc)
    {
        Quantity = Math.Max(0, quantity);
        UpdatedAtUtc = updatedAtUtc;
    }
}
