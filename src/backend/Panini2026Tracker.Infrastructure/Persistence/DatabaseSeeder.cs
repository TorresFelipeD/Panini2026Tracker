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

        if (await _catalogRepository.HasSeedDataAsync(cancellationToken))
        {
            return;
        }

        var seed = await _seedReader.ReadAsync(cancellationToken);
        var countries = seed.Countries
            .Select(country => new Country(country.Code, country.Name, country.DisplayOrder))
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
}
