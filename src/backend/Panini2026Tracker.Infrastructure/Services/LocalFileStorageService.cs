using Panini2026Tracker.Application.Abstractions;

namespace Panini2026Tracker.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    public async Task<string> SaveStickerImageAsync(Guid stickerId, Stream content, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension.ToLowerInvariant();
        var folderPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", stickerId.ToString("N"));
        Directory.CreateDirectory(folderPath);

        var storedFileName = $"sticker{safeExtension}";
        var filePath = Path.Combine(folderPath, storedFileName);

        await using var output = File.Create(filePath);
        await content.CopyToAsync(output, cancellationToken);

        return Path.Combine(stickerId.ToString("N"), storedFileName);
    }
}
