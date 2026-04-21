using System.Text.Json;
using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Application.Common;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Application.Stickers;

public sealed class StickerService : IStickerService
{
    private readonly IAlbumCatalogRepository _catalogRepository;
    private readonly IStickerCollectionRepository _collectionRepository;
    private readonly IStickerDuplicateRepository _duplicateRepository;
    private readonly ISystemLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StickerService(
        IAlbumCatalogRepository catalogRepository,
        IStickerCollectionRepository collectionRepository,
        IStickerDuplicateRepository duplicateRepository,
        ISystemLogRepository logRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _catalogRepository = catalogRepository;
        _collectionRepository = collectionRepository;
        _duplicateRepository = duplicateRepository;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<StickerDetailDto?> GetDetailAsync(Guid stickerId, CancellationToken cancellationToken)
    {
        var sticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken);
        return sticker is null ? null : Map(sticker);
    }

    public async Task<StickerDetailDto> UpdateStateAsync(Guid stickerId, UpdateStickerStateCommand command, CancellationToken cancellationToken)
    {
        var sticker = await _catalogRepository.GetStickerByIdAsync(stickerId, cancellationToken)
            ?? throw new InvalidOperationException("Sticker not found.");

        var now = _dateTimeProvider.UtcNow;
        var collectionEntry = await _collectionRepository.GetByStickerIdAsync(stickerId, cancellationToken);
        if (collectionEntry is null)
        {
            collectionEntry = new StickerCollectionEntry(stickerId, command.IsOwned, command.Notes, now);
            await _collectionRepository.AddAsync(collectionEntry, cancellationToken);
        }
        else
        {
            collectionEntry.Update(command.IsOwned, command.Notes, now);
        }

        var duplicateEntry = await _duplicateRepository.GetByStickerIdAsync(stickerId, cancellationToken);
        if (duplicateEntry is null && command.DuplicateCount > 0)
        {
            duplicateEntry = new StickerDuplicateEntry(stickerId, command.DuplicateCount, now);
            await _duplicateRepository.AddAsync(duplicateEntry, cancellationToken);
        }
        else if (duplicateEntry is not null)
        {
            if (command.DuplicateCount <= 0)
            {
                _duplicateRepository.Remove(duplicateEntry);
            }
            else
            {
                duplicateEntry.UpdateQuantity(command.DuplicateCount, now);
            }
        }

        await _logRepository.AddAsync(
            new SystemLog("stickers", "state.updated", $"Sticker {sticker.StickerCode} updated. Owned={command.IsOwned}, Duplicates={command.DuplicateCount}.", "info", now),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetDetailAsync(stickerId, cancellationToken))!;
    }

    private static StickerDetailDto Map(StickerCatalogItem sticker)
    {
        return new StickerDetailDto(
            sticker.Id,
            sticker.StickerCode,
            sticker.DisplayName,
            sticker.Country.Code,
            sticker.Country.Name,
            sticker.Type,
            sticker.CollectionEntry?.IsOwned ?? false,
            sticker.DuplicateEntry?.Quantity ?? 0,
            sticker.CollectionEntry?.Notes,
            sticker.StickerImage is null ? null : $"/uploads/{sticker.StickerImage.RelativePath.Replace("\\", "/")}",
            sticker.IsProvisional,
            DeserializeDictionary(sticker.AdditionalInfoJson),
            DeserializeDictionary(sticker.MetadataJson));
    }

    private static IReadOnlyDictionary<string, string> DeserializeDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonDefaults.SerializerOptions)
            ?? new Dictionary<string, string>();
    }
}
