using Panini2026Tracker.Domain.Common;

namespace Panini2026Tracker.Domain.Entities;

public sealed class Country : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string FlagCode { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public ICollection<StickerCatalogItem> Stickers { get; private set; } = new List<StickerCatalogItem>();

    private Country()
    {
    }

    public Country(string code, string name, string flagCode, int displayOrder)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        FlagCode = flagCode.Trim().ToLowerInvariant();
        DisplayOrder = displayOrder;
    }

    public void UpdateFlagCode(string flagCode)
    {
        FlagCode = flagCode.Trim().ToLowerInvariant();
    }

    public void UpdateCatalogData(string name, string flagCode, int displayOrder)
    {
        Name = name.Trim();
        FlagCode = flagCode.Trim().ToLowerInvariant();
        DisplayOrder = displayOrder;
    }
}
