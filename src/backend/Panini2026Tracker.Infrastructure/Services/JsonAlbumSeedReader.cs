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

        return new SeedCatalogDto(
            root.Countries.Select(country => new SeedCountryDto(
                country.Code,
                country.Group,
                country.Name,
                country.FlagCode,
                country.DisplayOrder,
                country.DisplayOrderGroup,
                country.Stickers.Select(sticker => MapSticker(sticker, sticker.Type, null)).ToArray())).ToArray(),
            root.Fcw?.Stickers.Select(sticker => MapSticker(sticker, "fcw", CreateSectionMetadata(root.Fcw.Group))).ToArray()
                ?? [],
            root.Extras.Select(sticker => MapSticker(sticker, sticker.Type, CreateSectionMetadata("EXTRA")))
                .Concat(root.Stickers.Select(sticker => MapSticker(sticker, sticker.Type, CreateSectionMetadata("EXTRA"))))
                .ToArray());
    }

    private static SeedStickerDto MapSticker(
        JsonSeedSticker sticker,
        string? fallbackType,
        Dictionary<string, string>? extraMetadata)
    {
        var metadata = sticker.Metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(sticker.Metadata, StringComparer.OrdinalIgnoreCase);

        if (extraMetadata is not null)
        {
            foreach (var pair in extraMetadata)
            {
                metadata[pair.Key] = pair.Value;
            }
        }

        metadata["isHorizontal"] = sticker.IsHorizontal ? "true" : "false";

        return new SeedStickerDto(
            sticker.StickerCode,
            sticker.DisplayName,
            string.IsNullOrWhiteSpace(sticker.Type) ? fallbackType ?? "extra" : sticker.Type,
            sticker.ImageReference,
            sticker.IsProvisional,
            sticker.DisplayOrder,
            sticker.Birthday,
            sticker.Height,
            sticker.Weight,
            sticker.Team,
            sticker.AdditionalInfo,
            metadata.Count == 0 ? null : metadata);
    }

    private static Dictionary<string, string> CreateSectionMetadata(string? section) =>
        string.IsNullOrWhiteSpace(section)
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["section"] = section
            };

    private sealed class JsonSeedRoot
    {
        public List<JsonSeedCountry> Countries { get; set; } = [];
        public JsonSeedSection? Fcw { get; set; }
        public List<JsonSeedSticker> Extras { get; set; } = [];
        public List<JsonSeedSticker> Stickers { get; set; } = [];
    }

    private sealed class JsonSeedCountry
    {
        public string Code { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FlagCode { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int DisplayOrderGroup { get; set; }
        public List<JsonSeedSticker> Stickers { get; set; } = [];
    }

    private sealed class JsonSeedSection
    {
        public string Group { get; set; } = string.Empty;
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
        public bool IsHorizontal { get; set; }
        public Dictionary<string, string>? AdditionalInfo { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
