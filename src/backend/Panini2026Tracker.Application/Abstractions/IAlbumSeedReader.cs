namespace Panini2026Tracker.Application.Abstractions;

public interface IAlbumSeedReader
{
    Task<SeedCatalogDto> ReadAsync(CancellationToken cancellationToken);
}

public sealed record SeedCatalogDto(IReadOnlyCollection<SeedCountryDto> Countries);

public sealed record SeedCountryDto(
    string Code,
    string Name,
    string FlagCode,
    int DisplayOrder,
    IReadOnlyCollection<SeedStickerDto> Stickers);

public sealed record SeedStickerDto(
    string StickerCode,
    string DisplayName,
    string Type,
    string? ImageReference,
    bool IsProvisional,
    int DisplayOrder,
    Dictionary<string, string>? AdditionalInfo,
    Dictionary<string, string>? Metadata);
