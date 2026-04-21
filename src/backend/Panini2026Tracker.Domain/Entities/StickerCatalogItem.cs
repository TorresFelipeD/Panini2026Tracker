using Panini2026Tracker.Domain.Common;

namespace Panini2026Tracker.Domain.Entities;

public sealed class StickerCatalogItem : BaseEntity
{
    public string StickerCode { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string? ImageReference { get; private set; }
    public string? AdditionalInfoJson { get; private set; }
    public string? MetadataJson { get; private set; }
    public bool IsProvisional { get; private set; }
    public int DisplayOrder { get; private set; }
    public Guid CountryId { get; private set; }

    public Country Country { get; private set; } = default!;
    public StickerCollectionEntry? CollectionEntry { get; private set; }
    public StickerDuplicateEntry? DuplicateEntry { get; private set; }
    public StickerImage? StickerImage { get; private set; }

    private StickerCatalogItem()
    {
    }

    public StickerCatalogItem(
        string stickerCode,
        string displayName,
        string type,
        string? imageReference,
        string? additionalInfoJson,
        string? metadataJson,
        bool isProvisional,
        int displayOrder,
        Guid countryId)
    {
        StickerCode = stickerCode.Trim().ToUpperInvariant();
        DisplayName = displayName.Trim();
        Type = type.Trim().ToLowerInvariant();
        ImageReference = imageReference?.Trim();
        AdditionalInfoJson = additionalInfoJson;
        MetadataJson = metadataJson;
        IsProvisional = isProvisional;
        DisplayOrder = displayOrder;
        CountryId = countryId;
    }
}
