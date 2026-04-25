using System.Text.Json;
using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Application.Common;

namespace Panini2026Tracker.Infrastructure.Services;

public sealed class JsonAlbumSeedReader : IAlbumSeedReader
{
    public async Task<SeedCatalogDto> ReadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Seed", "album-catalog.json");
        await using var stream = File.OpenRead(path);
        var root = await JsonSerializer.DeserializeAsync<JsonSeedRoot>(stream, JsonDefaults.SerializerOptions, cancellationToken)
            ?? throw new InvalidOperationException("Seed catalog could not be loaded.");

        return new SeedCatalogDto(root.Countries.Select(country => new SeedCountryDto(
            country.Code,
            country.Name,
            country.FlagCode,
            country.DisplayOrder,
            country.Stickers.Select(sticker => new SeedStickerDto(
                sticker.StickerCode,
                sticker.DisplayName,
                sticker.Type,
                sticker.ImageReference,
                sticker.IsProvisional,
                sticker.DisplayOrder,
                sticker.Birthday,
                sticker.Height,
                sticker.Weight,
                sticker.Team,
                sticker.AdditionalInfo,
                sticker.Metadata)).ToArray())).ToArray());
    }

    private sealed class JsonSeedRoot
    {
        public List<JsonSeedCountry> Countries { get; set; } = [];
    }

    private sealed class JsonSeedCountry
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FlagCode { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<JsonSeedSticker> Stickers { get; set; } = [];
    }

    private sealed class JsonSeedSticker
    {
        public string StickerCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ImageReference { get; set; }
        public bool IsProvisional { get; set; }
        public int DisplayOrder { get; set; }
        public string? Birthday { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Team { get; set; }
        public Dictionary<string, string>? AdditionalInfo { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
