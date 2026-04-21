using Panini2026Tracker.Domain.Common;

namespace Panini2026Tracker.Domain.Entities;

public sealed class StickerImage : BaseEntity
{
    public Guid StickerCatalogItemId { get; private set; }
    public string RelativePath { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public DateTime UploadedAtUtc { get; private set; }

    public StickerCatalogItem StickerCatalogItem { get; private set; } = default!;

    private StickerImage()
    {
    }

    public StickerImage(Guid stickerCatalogItemId, string relativePath, string originalFileName, string contentType, DateTime uploadedAtUtc)
    {
        StickerCatalogItemId = stickerCatalogItemId;
        RelativePath = relativePath;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        UploadedAtUtc = uploadedAtUtc;
    }

    public void Update(string relativePath, string originalFileName, string contentType, DateTime uploadedAtUtc)
    {
        RelativePath = relativePath;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        UploadedAtUtc = uploadedAtUtc;
    }
}
