namespace Panini2026Tracker.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveStickerImageAsync(Guid stickerId, Stream content, string fileName, CancellationToken cancellationToken);
}
