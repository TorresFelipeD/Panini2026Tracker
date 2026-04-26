namespace Panini2026Tracker.Application.Albums;

public sealed record AlbumFilter(
    string? Search,
    IReadOnlyCollection<string> CountryCodes,
    bool? IsOwned,
    bool? HasImage,
    bool? HasDuplicates);

public sealed record AlbumOverviewDto(
    AlbumSummaryDto Summary,
    IReadOnlyCollection<CountryAlbumDto> Countries,
    IReadOnlyCollection<SpecialStickerSectionDto> SpecialSections);

public sealed record AlbumSummaryDto(
    int Total,
    int Owned,
    int Missing,
    decimal CompletionPercentage);

public sealed record CountryAlbumDto(
    Guid CountryId,
    string CountryCode,
    string Group,
    int DisplayOrder,
    int DisplayOrderGroup,
    string CountryName,
    string FlagCode,
    int Total,
    int Owned,
    int Missing,
    decimal CompletionPercentage,
    IReadOnlyCollection<StickerCardDto> Stickers);

public sealed record SpecialStickerSectionDto(
    string Key,
    string Label,
    int Total,
    int Owned,
    int Missing,
    decimal CompletionPercentage,
    IReadOnlyCollection<StickerCardDto> Stickers);

public sealed record StickerCardDto(
    Guid StickerId,
    string StickerCode,
    string DisplayName,
    string CountryCode,
    string CountryName,
    string Type,
    bool IsOwned,
    bool HasImage,
    int DuplicateCount,
    string? ImageUrl,
    bool IsProvisional);

public interface IAlbumService
{
    Task<AlbumOverviewDto> GetOverviewAsync(AlbumFilter filter, CancellationToken cancellationToken);
}
