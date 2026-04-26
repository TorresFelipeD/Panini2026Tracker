namespace Panini2026Tracker.Application.Abstractions;

public interface IAlbumSeedReader
{
    Task<SeedCatalogDto> ReadAsync(CancellationToken cancellationToken);
}

public sealed record SeedCatalogDto(
    IReadOnlyCollection<SeedCountryDto> Countries,
    IReadOnlyCollection<SeedStickerDto> FcwStickers,
    IReadOnlyCollection<SeedStickerDto> ExtraStickers);

public sealed record SeedCountryDto(
    string Code,
    string Group,
    string Name,
    string FlagCode,
    int DisplayOrder,
    int DisplayOrderGroup,
    IReadOnlyCollection<SeedStickerDto> Stickers);

public sealed record SeedStickerDto(
    string StickerCode,
    string DisplayName,
    string Type,
    string? ImageReference,
    bool IsProvisional,
    int DisplayOrder,
    string? Birthday,
    string? Height,
    string? Weight,
    string? Team,
    Dictionary<string, string>? AdditionalInfo,
    Dictionary<string, string>? Metadata);
