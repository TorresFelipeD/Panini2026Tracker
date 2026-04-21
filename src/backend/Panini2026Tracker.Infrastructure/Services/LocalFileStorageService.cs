using Panini2026Tracker.Application.Abstractions;

namespace Panini2026Tracker.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService()
    {
        _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(_webRootPath);
    }

    public async Task<string> SaveStickerImageAsync(Guid stickerId, Stream content, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension.ToLowerInvariant();
        var folderPath = Path.Combine(_webRootPath, "uploads", stickerId.ToString("N"));
        Directory.CreateDirectory(folderPath);

        var storedFileName = $"sticker{safeExtension}";
        var filePath = Path.Combine(folderPath, storedFileName);

        await using var output = File.Create(filePath);
        await content.CopyToAsync(output, cancellationToken);

        return Path.Combine(stickerId.ToString("N"), storedFileName);
    }

    public Task DeleteStickerImageAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var normalizedRelativePath = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_webRootPath, "uploads", normalizedRelativePath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(_webRootPath, "uploads"));

        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid image path.");
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        var folderPath = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath) && !Directory.EnumerateFileSystemEntries(folderPath).Any())
        {
            Directory.Delete(folderPath);
        }

        return Task.CompletedTask;
    }
}
