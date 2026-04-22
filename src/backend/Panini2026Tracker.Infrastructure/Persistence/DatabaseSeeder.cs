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
        var seed = await _seedReader.ReadAsync(cancellationToken);

        if (await _catalogRepository.HasSeedDataAsync(cancellationToken))
        {
            var existingCountries = await _dbContext.Countries.ToListAsync(cancellationToken);
            foreach (var seedCountry in seed.Countries)
            {
                var existingCountry = existingCountries.FirstOrDefault(country =>
                    country.Code.Equals(seedCountry.Code, StringComparison.OrdinalIgnoreCase));

                if (existingCountry is not null
                    && !string.Equals(existingCountry.FlagCode, seedCountry.FlagCode, StringComparison.Ordinal))
                {
                    existingCountry.UpdateFlagCode(seedCountry.FlagCode);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var countries = seed.Countries
            .Select(country => new Country(country.Code, country.Name, country.FlagCode, country.DisplayOrder))
            .ToArray();

        await _catalogRepository.AddCountriesAsync(countries, cancellationToken);

        var countryMap = countries.ToDictionary(country => country.Code, StringComparer.OrdinalIgnoreCase);
        var stickers = seed.Countries
            .SelectMany(country => country.Stickers.Select(sticker => new StickerCatalogItem(
                sticker.StickerCode,
                sticker.DisplayName,
                sticker.Type,
                sticker.ImageReference,
                sticker.AdditionalInfo is null ? null : JsonSerializer.Serialize(sticker.AdditionalInfo, JsonDefaults.SerializerOptions),
                sticker.Metadata is null ? null : JsonSerializer.Serialize(sticker.Metadata, JsonDefaults.SerializerOptions),
                sticker.IsProvisional,
                sticker.DisplayOrder,
                countryMap[country.Code].Id)))
            .ToArray();

        await _catalogRepository.AddStickersAsync(stickers, cancellationToken);
        await _logRepository.AddAsync(new SystemLog("seed", "catalog.seeded", $"Seeded {stickers.Length} stickers across {countries.Length} countries.", "info", DateTime.UtcNow), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
}
