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
        var additionalInfo = new Dictionary<string, string>(DeserializeDictionary(sticker.AdditionalInfoJson));

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

        if (string.Equals(sticker.Type, "jugador", StringComparison.OrdinalIgnoreCase))
        {
            additionalInfo["birthday"] = command.Birthday?.Trim() ?? string.Empty;
            additionalInfo["height"] = command.Height?.Trim() ?? string.Empty;
            additionalInfo["weight"] = command.Weight?.Trim() ?? string.Empty;
            additionalInfo["team"] = command.Team?.Trim() ?? string.Empty;
            sticker.UpdateAdditionalInfo(JsonSerializer.Serialize(additionalInfo, JsonDefaults.SerializerOptions));
        }

        await _logRepository.AddAsync(
            new SystemLog("stickers", "state.updated", $"Sticker {sticker.StickerCode} updated. Owned={command.IsOwned}, Duplicates={command.DuplicateCount}.", "info", now),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetDetailAsync(stickerId, cancellationToken))!;
    }

    private static StickerDetailDto Map(StickerCatalogItem sticker)
    {
        var additionalInfo = DeserializeDictionary(sticker.AdditionalInfoJson);

        return new StickerDetailDto(
            sticker.Id,
            sticker.StickerCode,
            sticker.DisplayName,
            sticker.Country?.Code,
            sticker.Country?.Name,
            sticker.Country?.FlagCode,
            sticker.Type,
            sticker.CollectionEntry?.IsOwned ?? false,
            sticker.DuplicateEntry?.Quantity ?? 0,
            sticker.CollectionEntry?.Notes,
            ImageUrlBuilder.Build(sticker.StickerImage),
            sticker.IsProvisional,
            GetFieldValue(additionalInfo, "birthday"),
            GetFieldValue(additionalInfo, "height"),
            GetFieldValue(additionalInfo, "weight"),
            GetFieldValue(additionalInfo, "team"),
            RemoveReservedPlayerFields(additionalInfo),
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

    private static string GetFieldValue(IReadOnlyDictionary<string, string> source, string key) =>
        source.TryGetValue(key, out var value) ? value : string.Empty;

    private static IReadOnlyDictionary<string, string> RemoveReservedPlayerFields(IReadOnlyDictionary<string, string> source)
    {
        var filtered = source
            .Where(pair => pair.Key is not ("birthday" or "height" or "weight" or "team"))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return filtered;
    }
}
