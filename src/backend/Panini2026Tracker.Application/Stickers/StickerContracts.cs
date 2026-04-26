namespace Panini2026Tracker.Application.Stickers;

public sealed record StickerDetailDto(
    Guid StickerId,
    string StickerCode,
    string DisplayName,
    string? CountryCode,
    string? CountryName,
    string? FlagCode,
    string Type,
    bool IsOwned,
    int DuplicateCount,
    string? Notes,
    string? ImageUrl,
    bool IsProvisional,
    string Birthday,
    string Height,
    string Weight,
    string Team,
    IReadOnlyDictionary<string, string> AdditionalInfo,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record UpdateStickerStateCommand(
    bool IsOwned,
    int DuplicateCount,
    string? Notes,
    string? Birthday,
    string? Height,
    string? Weight,
    string? Team);

public interface IStickerService
{
    Task<StickerDetailDto?> GetDetailAsync(Guid stickerId, CancellationToken cancellationToken);
    Task<StickerDetailDto> UpdateStateAsync(Guid stickerId, UpdateStickerStateCommand command, CancellationToken cancellationToken);
}
