namespace Panini2026Tracker.Application.Albums;

public sealed record AlbumFilter(
    string? Search,
    string? CountryCode,
    bool? IsOwned,
    bool? HasImage,
    bool? HasDuplicates);

public sealed record AlbumOverviewDto(
    AlbumSummaryDto Summary,
    IReadOnlyCollection<CountryAlbumDto> Countries);

public sealed record AlbumSummaryDto(
    int Total,
    int Owned,
    int Missing,
    decimal CompletionPercentage);

public sealed record CountryAlbumDto(
    Guid CountryId,
    string CountryCode,
    string CountryName,
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
