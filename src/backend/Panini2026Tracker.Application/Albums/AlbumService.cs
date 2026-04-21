using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Application.Albums;

public sealed class AlbumService : IAlbumService
{
    private readonly IAlbumCatalogRepository _catalogRepository;

    public AlbumService(IAlbumCatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<AlbumOverviewDto> GetOverviewAsync(AlbumFilter filter, CancellationToken cancellationToken)
    {
        var countries = await _catalogRepository.GetCountriesWithStickersAsync(cancellationToken);
        var countryDtos = countries
            .Select(country =>
            {
                var stickers = country.Stickers
                    .Where(sticker => Matches(sticker, filter))
                    .OrderBy(sticker => sticker.DisplayOrder)
                    .Select(sticker => new StickerCardDto(
                        sticker.Id,
                        sticker.StickerCode,
                        sticker.DisplayName,
                        country.Code,
                        country.Name,
                        sticker.Type,
                        sticker.CollectionEntry?.IsOwned ?? false,
                        sticker.StickerImage is not null,
                        sticker.DuplicateEntry?.Quantity ?? 0,
                        sticker.StickerImage is null ? null : $"/uploads/{sticker.StickerImage.RelativePath.Replace("\\", "/")}",
                        sticker.IsProvisional))
                    .ToArray();

                var total = stickers.Length;
                var owned = stickers.Count(sticker => sticker.IsOwned);

                return new CountryAlbumDto(
                    country.Id,
                    country.Code,
                    country.Name,
                    total,
                    owned,
                    total - owned,
                    total == 0 ? 0 : decimal.Round((decimal)owned / total * 100, 1),
                    stickers);
            })
            .Where(country => country.Stickers.Count > 0)
            .OrderBy(country => country.CountryName)
            .ToArray();

        var totalCount = countryDtos.Sum(country => country.Total);
        var ownedCount = countryDtos.Sum(country => country.Owned);

        return new AlbumOverviewDto(
            new AlbumSummaryDto(
                totalCount,
                ownedCount,
                totalCount - ownedCount,
                totalCount == 0 ? 0 : decimal.Round((decimal)ownedCount / totalCount * 100, 1)),
            countryDtos);
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
                sticker.Country.Code,
                sticker.Country.Name,
                sticker.AdditionalInfoJson ?? string.Empty,
                sticker.MetadataJson ?? string.Empty)
            .ToLowerInvariant();

        return (string.IsNullOrWhiteSpace(filter.Search) || blob.Contains(filter.Search.Trim().ToLowerInvariant()))
            && (string.IsNullOrWhiteSpace(filter.CountryCode) || sticker.Country.Code.Equals(filter.CountryCode.Trim(), StringComparison.OrdinalIgnoreCase))
            && (!filter.IsOwned.HasValue || owned == filter.IsOwned.Value)
            && (!filter.HasImage.HasValue || hasImage == filter.HasImage.Value)
            && (!filter.HasDuplicates.HasValue || hasDuplicates == filter.HasDuplicates.Value);
    }
}
