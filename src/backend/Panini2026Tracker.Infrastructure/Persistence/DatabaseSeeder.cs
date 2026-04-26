using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Application.Common;
using Panini2026Tracker.Domain.Entities;
using Panini2026Tracker.Domain.Repositories;

namespace Panini2026Tracker.Infrastructure.Persistence;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IAlbumCatalogRepository _catalogRepository;
    private readonly IAlbumSeedReader _seedReader;
    private readonly ISystemLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseSeeder(
        AppDbContext dbContext,
        IAlbumCatalogRepository catalogRepository,
        IAlbumSeedReader seedReader,
        ISystemLogRepository logRepository,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _catalogRepository = catalogRepository;
        _seedReader = seedReader;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        await EnsureCountryFlagCodeColumnAsync(cancellationToken);
        await EnsureStickerCountryIdNullableAsync(cancellationToken);
        var seed = await _seedReader.ReadAsync(cancellationToken);
        var hadSeedData = await _catalogRepository.HasSeedDataAsync(cancellationToken);

        var existingCountries = await _dbContext.Countries.ToListAsync(cancellationToken);
        var countryMap = existingCountries.ToDictionary(country => country.Code, StringComparer.OrdinalIgnoreCase);
        var addedCountries = new List<Country>();

        foreach (var seedCountry in seed.Countries)
        {
            if (countryMap.TryGetValue(seedCountry.Code, out var existingCountry))
            {
                existingCountry.UpdateCatalogData(seedCountry.Name, seedCountry.FlagCode, seedCountry.DisplayOrder);
                continue;
            }

            var newCountry = new Country(seedCountry.Code, seedCountry.Name, seedCountry.FlagCode, seedCountry.DisplayOrder);
            addedCountries.Add(newCountry);
            countryMap[newCountry.Code] = newCountry;
        }

        if (addedCountries.Count > 0)
        {
            await _catalogRepository.AddCountriesAsync(addedCountries, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var existingStickers = await _dbContext.StickerCatalogItems.ToListAsync(cancellationToken);
        var stickerMap = existingStickers
            .GroupBy(sticker => sticker.StickerCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var stickerSlotMap = existingStickers
            .Where(sticker => sticker.CountryId.HasValue)
            .GroupBy(sticker => BuildStickerSlotKey(sticker.CountryId, sticker.DisplayOrder), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var addedStickers = new List<StickerCatalogItem>();

        foreach (var seedCountry in seed.Countries)
        {
            var countryId = countryMap[seedCountry.Code].Id;

            foreach (var seedSticker in seedCountry.Stickers)
            {
                UpsertSticker(seedSticker, countryId, stickerMap, stickerSlotMap, addedStickers);
            }
        }

        foreach (var seedSticker in seed.FcwStickers)
        {
            UpsertSticker(seedSticker, null, stickerMap, stickerSlotMap, addedStickers);
        }

        foreach (var seedSticker in seed.ExtraStickers)
        {
            UpsertSticker(seedSticker with
            {
                Type = string.IsNullOrWhiteSpace(seedSticker.Type) ? "extra" : seedSticker.Type
            }, null, stickerMap, stickerSlotMap, addedStickers);
        }

        if (addedStickers.Count > 0)
        {
            await _catalogRepository.AddStickersAsync(addedStickers, cancellationToken);
        }

        var action = hadSeedData ? "catalog.synced" : "catalog.seeded";
        var totalSeedStickers =
            seed.Countries.Sum(country => country.Stickers.Count)
            + seed.FcwStickers.Count
            + seed.ExtraStickers.Count;
        var details = $"Synchronized catalog with {seed.Countries.Count} countries, {totalSeedStickers} stickers, {addedCountries.Count} new countries and {addedStickers.Count} new stickers.";
        await _logRepository.AddAsync(new SystemLog("seed", action, details, "info", DateTime.UtcNow), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string BuildStickerSlotKey(Guid? countryId, int displayOrder) => $"{countryId}:{displayOrder}";

    private static void UpsertSticker(
        SeedStickerDto seedSticker,
        Guid? countryId,
        IDictionary<string, StickerCatalogItem> stickerMap,
        IDictionary<string, StickerCatalogItem> stickerSlotMap,
        ICollection<StickerCatalogItem> addedStickers)
    {
        var additionalInfo = BuildAdditionalInfo(seedSticker);
        var additionalInfoJson = additionalInfo.Count == 0
            ? null
            : JsonSerializer.Serialize(additionalInfo, JsonDefaults.SerializerOptions);
        var metadataJson = seedSticker.Metadata is null
            ? null
            : JsonSerializer.Serialize(seedSticker.Metadata, JsonDefaults.SerializerOptions);

        stickerMap.TryGetValue(seedSticker.StickerCode, out var existingSticker);

        if (existingSticker is null && countryId.HasValue)
        {
            stickerSlotMap.TryGetValue(BuildStickerSlotKey(countryId, seedSticker.DisplayOrder), out existingSticker);
        }

        if (existingSticker is not null)
        {
            existingSticker.UpdateCatalogData(
                seedSticker.StickerCode,
                seedSticker.DisplayName,
                seedSticker.Type,
                seedSticker.ImageReference,
                additionalInfoJson,
                metadataJson,
                seedSticker.IsProvisional,
                seedSticker.DisplayOrder,
                countryId);

            stickerMap[existingSticker.StickerCode] = existingSticker;
            if (countryId.HasValue)
            {
                stickerSlotMap[BuildStickerSlotKey(countryId, seedSticker.DisplayOrder)] = existingSticker;
            }

            return;
        }

        var newSticker = new StickerCatalogItem(
            seedSticker.StickerCode,
            seedSticker.DisplayName,
            seedSticker.Type,
            seedSticker.ImageReference,
            additionalInfoJson,
            metadataJson,
            seedSticker.IsProvisional,
            seedSticker.DisplayOrder,
            countryId);

        addedStickers.Add(newSticker);
        stickerMap[newSticker.StickerCode] = newSticker;
        if (countryId.HasValue)
        {
            stickerSlotMap[BuildStickerSlotKey(countryId, seedSticker.DisplayOrder)] = newSticker;
        }
    }

    private static Dictionary<string, string> BuildAdditionalInfo(SeedStickerDto seedSticker)
    {
        var result = seedSticker.AdditionalInfo is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(seedSticker.AdditionalInfo, StringComparer.OrdinalIgnoreCase);

        if (string.Equals(seedSticker.Type, "jugador", StringComparison.OrdinalIgnoreCase))
        {
            result["birthday"] = seedSticker.Birthday ?? string.Empty;
            result["height"] = seedSticker.Height ?? string.Empty;
            result["weight"] = seedSticker.Weight ?? string.Empty;
            result["team"] = seedSticker.Team ?? string.Empty;
        }

        return result;
    }

    private async Task EnsureCountryFlagCodeColumnAsync(CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info('Countries');";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var hasFlagEmojiColumn = false;
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader["name"]?.ToString(), "FlagEmoji", StringComparison.OrdinalIgnoreCase))
                {
                    hasFlagEmojiColumn = true;
                    break;
                }
            }

            if (!hasFlagEmojiColumn)
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE Countries ADD COLUMN FlagEmoji TEXT NOT NULL DEFAULT '';",
                    cancellationToken);
            }
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task EnsureStickerCountryIdNullableAsync(CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;
        var shouldRebuildTable = false;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('StickerCatalogItems');";

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (string.Equals(reader["name"]?.ToString(), "CountryId", StringComparison.OrdinalIgnoreCase))
                    {
                        shouldRebuildTable = Convert.ToInt32(reader["notnull"]) == 1;
                        break;
                    }
                }
            }

            if (!shouldRebuildTable)
            {
                return;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await _dbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE "StickerCatalogItems_tmp" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_StickerCatalogItems" PRIMARY KEY,
                    "StickerCode" TEXT NOT NULL,
                    "DisplayName" TEXT NOT NULL,
                    "Type" TEXT NOT NULL,
                    "ImageReference" TEXT NULL,
                    "AdditionalInfoJson" TEXT NULL,
                    "MetadataJson" TEXT NULL,
                    "IsProvisional" INTEGER NOT NULL,
                    "DisplayOrder" INTEGER NOT NULL,
                    "CountryId" TEXT NULL,
                    CONSTRAINT "FK_StickerCatalogItems_Countries_CountryId"
                        FOREIGN KEY ("CountryId") REFERENCES "Countries" ("Id") ON DELETE CASCADE
                );
                """,
                cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "StickerCatalogItems_tmp" (
                    "Id",
                    "StickerCode",
                    "DisplayName",
                    "Type",
                    "ImageReference",
                    "AdditionalInfoJson",
                    "MetadataJson",
                    "IsProvisional",
                    "DisplayOrder",
                    "CountryId"
                )
                SELECT
                    "Id",
                    "StickerCode",
                    "DisplayName",
                    "Type",
                    "ImageReference",
                    "AdditionalInfoJson",
                    "MetadataJson",
                    "IsProvisional",
                    "DisplayOrder",
                    "CountryId"
                FROM "StickerCatalogItems";
                """,
                cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("DROP TABLE \"StickerCatalogItems\";", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE \"StickerCatalogItems_tmp\" RENAME TO \"StickerCatalogItems\";", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX \"IX_StickerCatalogItems_StickerCode\" ON \"StickerCatalogItems\" (\"StickerCode\");", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("CREATE INDEX \"IX_StickerCatalogItems_CountryId\" ON \"StickerCatalogItems\" (\"CountryId\");", cancellationToken);
            await _dbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;", cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
