using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Application.Common;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Application.Images;

public sealed class ImageService : IImageService
{
    private readonly IAlbumCatalogRepository _catalogRepository;
    private readonly IStickerImageRepository _imageRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISystemLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ImageService(
        IAlbumCatalogRepository catalogRepository,
        IStickerImageRepository imageRepository,
        IFileStorageService fileStorageService,
        ISystemLogRepository logRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _catalogRepository = catalogRepository;
        _imageRepository = imageRepository;
        _fileStorageService = fileStorageService;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<StickerImageDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var stickers = await _catalogRepository.GetStickersWithRelationsAsync(cancellationToken);
        return stickers
            .Where(sticker => sticker.StickerImage is not null)
            .OrderBy(sticker => GetGroupSortKey(sticker))
            .ThenBy(sticker => sticker.Country?.Name ?? sticker.DisplayName)
            .ThenBy(sticker => sticker.StickerCode)
            .Select(sticker =>
            {
                var image = sticker.StickerImage!;

                return new StickerImageDto(
                    sticker.Id,
                    sticker.StickerCode,
                    sticker.Country?.Code,
                    sticker.Country?.Name,
                    sticker.Country?.FlagCode,
                    sticker.DisplayName,
                    sticker.Type,
                    ImageUrlBuilder.Build(image)!,
                    image.UploadedAtUtc);
            })
            .ToArray();
    }

    public async Task<StickerImageDto> UploadAsync(Guid stickerId, Stream content, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var sticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Sticker not found.");

        var relativePath = await _fileStorageService.SaveStickerImageAsync(stickerId, content, fileName, cancellationToken);
        var now = _dateTimeProvider.UtcNow;
        var existingImage = await _imageRepository.GetByStickerIdAsync(stickerId, cancellationToken);
        if (existingImage is null)
        {
            existingImage = new StickerImage(stickerId, relativePath, fileName, contentType, now);
            await _imageRepository.AddAsync(existingImage, cancellationToken);
        }
        else
        {
            existingImage.Update(relativePath, fileName, contentType, now);
        }

        await _logRepository.AddAsync(
            new SystemLog("images", "images.uploaded", $"Image uploaded for sticker {sticker.StickerCode}.", "info", now),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StickerImageDto(
            sticker.Id,
            sticker.StickerCode,
            sticker.Country?.Code,
            sticker.Country?.Name,
            sticker.Country?.FlagCode,
            sticker.DisplayName,
            sticker.Type,
            ImageUrlBuilder.Build(existingImage)!,
            now);
    }

    public async Task DeleteAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        var sticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Sticker not found.");
        var existingImage = await _imageRepository.GetByStickerIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Image not found.");

        await _fileStorageService.DeleteStickerImageAsync(existingImage.RelativePath, cancellationToken);
        _imageRepository.Remove(existingImage);

        var now = _dateTimeProvider.UtcNow;
        await _logRepository.AddAsync(
            new SystemLog("images", "images.deleted", $"Image deleted for sticker {sticker.StickerCode}.", "info", now),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static int GetGroupSortKey(StickerCatalogItem sticker)
    {
        if (sticker.Country is not null)
        {
            return 0;
        }

        return string.Equals(sticker.Type, "fcw", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
    }
}
