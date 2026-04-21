using Panini2026Tracker.Domain.Common;

namespace Panini2026Tracker.Domain.Entities;

public sealed class StickerCollectionEntry : BaseEntity
{
    public Guid StickerCatalogItemId { get; private set; }
    public bool IsOwned { get; private set; }
    public string? Notes { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public StickerCatalogItem StickerCatalogItem { get; private set; } = default!;

    private StickerCollectionEntry()
    {
    }

    public StickerCollectionEntry(Guid stickerCatalogItemId, bool isOwned, string? notes, DateTime updatedAtUtc)
    {
        StickerCatalogItemId = stickerCatalogItemId;
        IsOwned = isOwned;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Update(bool isOwned, string? notes, DateTime updatedAtUtc)
    {
        IsOwned = isOwned;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAtUtc = updatedAtUtc;
    }
}
