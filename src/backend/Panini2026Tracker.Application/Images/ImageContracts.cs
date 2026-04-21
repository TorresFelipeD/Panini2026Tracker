namespace Panini2026Tracker.Application.Images;

public sealed record StickerImageDto(
    Guid StickerId,
    string StickerCode,
    string CountryCode,
    string CountryName,
    string DisplayName,
    string ImageUrl,
    DateTime UploadedAtUtc);

public interface IImageService
{
    Task<IReadOnlyCollection<StickerImageDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<StickerImageDto> UploadAsync(Guid stickerId, Stream content, string fileName, string contentType, CancellationToken cancellationToken);
}
