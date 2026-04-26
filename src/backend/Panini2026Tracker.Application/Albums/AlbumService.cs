using Panini2026Tracker.Application.Common;
using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Application.Albums;

public sealed class AlbumService : IAlbumService
{
    private readonly IAlbumCatalogRepository _catalogRepository;
    private readonly IAlbumSeedReader _seedReader;

    public AlbumService(IAlbumCatalogRepository catalogRepository, IAlbumSeedReader seedReader)
    {
        _catalogRepository = catalogRepository;
        _seedReader = seedReader;
    }

    public async Task<AlbumOverviewDto> GetOverviewAsync(AlbumFilter filter, CancellationToken cancellationToken)
    {
        var stickers = await _catalogRepository.GetStickersWithRelationsAsync(cancellationToken);
        var seed = await _seedReader.ReadAsync(cancellationToken);
        var countryGroupMap = seed.Countries.ToDictionary(country => country.Code, country => country.Group, StringComparer.OrdinalIgnoreCase);

        var matchedStickers = stickers
            .Where(sticker => Matches(sticker, filter))
            .ToArray();

        var countryDtos = matchedStickers
            .Where(sticker => sticker.CountryId.HasValue && sticker.Country is not null)
            .GroupBy(sticker => sticker.CountryId!.Value)
            .Select(group =>
            {
                var firstSticker = group.First();
                var country = firstSticker.Country!;
                var cards = group
                    .OrderBy(sticker => sticker.DisplayOrder)
                    .ThenBy(sticker => sticker.StickerCode)
                    .Select(MapStickerCard)
                    .ToArray();

                return CreateCountryAlbumDto(
                    country.Id,
                    country.Code,
                    countryGroupMap.TryGetValue(country.Code, out var countryGroup) ? countryGroup : string.Empty,
                    country.Name,
                    country.FlagCode,
                    cards);
            })
            .OrderBy(country => country.CountryName)
            .ToArray();

        var specialSections = CreateSpecialSections(matchedStickers);
        var totalCount = countryDtos.Sum(country => country.Total) + specialSections.Sum(section => section.Total);
        var ownedCount = countryDtos.Sum(country => country.Owned) + specialSections.Sum(section => section.Owned);

        return new AlbumOverviewDto(
            new AlbumSummaryDto(
                totalCount,
                ownedCount,
                totalCount - ownedCount,
                totalCount == 0 ? 0 : decimal.Round((decimal)ownedCount / totalCount * 100, 1)),
            countryDtos,
            specialSections);
    }

    private static bool Matches(Domain.Entities.StickerCatalogItem sticker, AlbumFilter filter)
    {
        var owned = sticker.CollectionEntry?.IsOwned ?? false;
        var hasImage = sticker.StickerImage is not null;
        var hasDuplicates = (sticker.DuplicateEntry?.Quantity ?? 0) > 0;
        var blob = string.Join(
                ' ',
                sticker.StickerCode,
                sticker.DisplayName,
                sticker.Type,
                sticker.Country?.Code ?? string.Empty,
                sticker.Country?.Name ?? string.Empty,
                sticker.AdditionalInfoJson ?? string.Empty,
                sticker.MetadataJson ?? string.Empty)
            .ToLowerInvariant();

        return (string.IsNullOrWhiteSpace(filter.Search) || blob.Contains(filter.Search.Trim().ToLowerInvariant()))
            && (filter.CountryCodes.Count == 0 || (sticker.Country is not null && filter.CountryCodes.Contains(sticker.Country.Code, StringComparer.OrdinalIgnoreCase)))
            && (!filter.IsOwned.HasValue || owned == filter.IsOwned.Value)
            && (!filter.HasImage.HasValue || hasImage == filter.HasImage.Value)
            && (!filter.HasDuplicates.HasValue || hasDuplicates == filter.HasDuplicates.Value);
    }

    private static CountryAlbumDto CreateCountryAlbumDto(
        Guid countryId,
        string countryCode,
        string group,
        string countryName,
        string flagCode,
        IReadOnlyCollection<StickerCardDto> stickers)
    {
        var total = stickers.Count;
        var owned = stickers.Count(sticker => sticker.IsOwned);

        return new CountryAlbumDto(
            countryId,
            countryCode,
            group,
            countryName,
            flagCode,
            total,
            owned,
            total - owned,
            total == 0 ? 0 : decimal.Round((decimal)owned / total * 100, 1),
            stickers);
    }

    private static IReadOnlyCollection<SpecialStickerSectionDto> CreateSpecialSections(
        IReadOnlyCollection<Domain.Entities.StickerCatalogItem> stickers)
    {
        var sections = new List<SpecialStickerSectionDto>();

        var fcwStickers = stickers
            .Where(IsFcwSticker)
            .OrderBy(sticker => sticker.DisplayOrder)
            .ThenBy(sticker => sticker.StickerCode)
            .Select(MapStickerCard)
            .ToArray();

        if (fcwStickers.Length > 0)
        {
            sections.Add(CreateSpecialStickerSectionDto("fcw", "FCW", fcwStickers));
        }

        var extraStickers = stickers
            .Where(IsExtraSticker)
            .OrderBy(sticker => sticker.DisplayOrder)
            .ThenBy(sticker => sticker.StickerCode)
            .Select(MapStickerCard)
            .ToArray();

        if (extraStickers.Length > 0)
        {
            sections.Add(CreateSpecialStickerSectionDto("other", "Otros", extraStickers));
        }

        return sections;
    }

    private static SpecialStickerSectionDto CreateSpecialStickerSectionDto(
        string key,
        string label,
        IReadOnlyCollection<StickerCardDto> stickers)
    {
        var total = stickers.Count;
        var owned = stickers.Count(sticker => sticker.IsOwned);

        return new SpecialStickerSectionDto(
            key,
            label,
            total,
            owned,
            total - owned,
            total == 0 ? 0 : decimal.Round((decimal)owned / total * 100, 1),
            stickers);
    }

    private static StickerCardDto MapStickerCard(Domain.Entities.StickerCatalogItem sticker) =>
        new(
            sticker.Id,
            sticker.StickerCode,
            sticker.DisplayName,
            sticker.Country?.Code ?? string.Empty,
            sticker.Country?.Name ?? string.Empty,
            sticker.Type,
            sticker.CollectionEntry?.IsOwned ?? false,
            sticker.StickerImage is not null,
            sticker.DuplicateEntry?.Quantity ?? 0,
            ImageUrlBuilder.Build(sticker.StickerImage),
            sticker.IsProvisional);

    private static bool IsFcwSticker(Domain.Entities.StickerCatalogItem sticker) =>
        string.Equals(sticker.Type, "fcw", StringComparison.OrdinalIgnoreCase);

    private static bool IsExtraSticker(Domain.Entities.StickerCatalogItem sticker) =>
        sticker.CountryId is null && !IsFcwSticker(sticker);
}
