using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Application.Duplicates;

public sealed class DuplicateService : IDuplicateService
{
    private readonly IAlbumCatalogRepository _catalogRepository;
    private readonly IStickerDuplicateRepository _duplicateRepository;
    private readonly ISystemLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DuplicateService(
        IAlbumCatalogRepository catalogRepository,
        IStickerDuplicateRepository duplicateRepository,
        ISystemLogRepository logRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _catalogRepository = catalogRepository;
        _duplicateRepository = duplicateRepository;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<DuplicateItemDto>> GetAllAsync(DuplicateFilter filter, CancellationToken cancellationToken)
    {
        var stickers = await _catalogRepository.GetStickersWithRelationsAsync(cancellationToken);
        return stickers
            .Where(sticker => (sticker.DuplicateEntry?.Quantity ?? 0) > 0)
            .Where(sticker => filter.CountryCodes.Count == 0
                || filter.CountryCodes.Contains(GetGroupKey(sticker), StringComparer.OrdinalIgnoreCase))
            .Where(sticker => string.IsNullOrWhiteSpace(filter.Search)
                || $"{sticker.StickerCode} {sticker.DisplayName} {GetGroupLabel(sticker)} {sticker.Type}".Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(sticker => GetGroupSortKey(sticker))
            .ThenBy(sticker => GetGroupLabel(sticker))
            .ThenBy(sticker => sticker.StickerCode)
            .Select(sticker => new DuplicateItemDto(
                sticker.Id,
                sticker.StickerCode,
                sticker.DisplayName,
                sticker.Country?.Code,
                sticker.Country?.Name,
                sticker.Country?.FlagCode,
                sticker.Type,
                sticker.DuplicateEntry?.Quantity ?? 0,
                sticker.CollectionEntry?.IsOwned ?? false))
            .ToArray();
    }

    public async Task<DuplicateItemDto> UpdateAsync(Guid stickerId, UpdateDuplicateCommand command, CancellationToken cancellationToken)
    {
        var sticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Sticker not found.");

        var entry = await _duplicateRepository.GetByStickerIdAsync(stickerId, cancellationToken);
        if (command.Quantity <= 0)
        {
            if (entry is not null)
            {
                _duplicateRepository.Remove(entry);
            }
        }
        else if (entry is null)
        {
            entry = new StickerDuplicateEntry(stickerId, command.Quantity, _dateTimeProvider.UtcNow);
            await _duplicateRepository.AddAsync(entry, cancellationToken);
        }
        else
        {
            entry.UpdateQuantity(command.Quantity, _dateTimeProvider.UtcNow);
        }

        await _logRepository.AddAsync(
            new SystemLog("duplicates", "duplicates.updated", $"Sticker {sticker.StickerCode} duplicates set to {Math.Max(0, command.Quantity)}.", "info", _dateTimeProvider.UtcNow),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedSticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Sticker not found.");

        return new DuplicateItemDto(
            updatedSticker.Id,
            updatedSticker.StickerCode,
            updatedSticker.DisplayName,
            updatedSticker.Country?.Code,
            updatedSticker.Country?.Name,
            updatedSticker.Country?.FlagCode,
            updatedSticker.Type,
            updatedSticker.DuplicateEntry?.Quantity ?? 0,
            updatedSticker.CollectionEntry?.IsOwned ?? false);
    }

    public async Task DeleteAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        var sticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Sticker not found.");
        var entry = await _duplicateRepository.GetByStickerIdAsync(stickerId, cancellationToken);
        if (entry is null)
        {
            return;
        }

        _duplicateRepository.Remove(entry);
        await _logRepository.AddAsync(
            new SystemLog("duplicates", "duplicates.deleted", $"Sticker {sticker.StickerCode} duplicate record removed.", "warning", _dateTimeProvider.UtcNow),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GetGroupKey(StickerCatalogItem sticker)
    {
        if (sticker.Country?.Code is { Length: > 0 } countryCode)
        {
            return countryCode;
        }

        return string.Equals(sticker.Type, "fcw", StringComparison.OrdinalIgnoreCase) ? "fcw" : "otros";
    }

    private static string GetGroupLabel(StickerCatalogItem sticker)
    {
        if (sticker.Country?.Name is { Length: > 0 } countryName)
        {
            return countryName;
        }

        return string.Equals(sticker.Type, "fcw", StringComparison.OrdinalIgnoreCase) ? "FCW" : "Otros";
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
