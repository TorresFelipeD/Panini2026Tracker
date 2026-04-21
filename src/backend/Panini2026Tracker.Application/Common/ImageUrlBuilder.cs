using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Application.Common;

internal static class ImageUrlBuilder
{
    public static string? Build(StickerImage? image)
    {
        if (image is null)
        {
            return null;
        }

        var relativePath = image.RelativePath.Replace("\\", "/");
        return $"/uploads/{relativePath}?v={image.UploadedAtUtc.Ticks}";
    }
}
