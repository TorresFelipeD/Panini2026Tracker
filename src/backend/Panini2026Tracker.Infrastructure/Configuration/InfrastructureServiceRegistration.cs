using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Panini2026Tracker.Application.Abstractions;
using Panini2026Tracker.Domain.Repositories;
using Panini2026Tracker.Infrastructure.Persistence;
using Panini2026Tracker.Infrastructure.Repositories;
using Panini2026Tracker.Infrastructure.Services;

namespace Panini2026Tracker.Infrastructure.Configuration;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var sqliteConnectionString = ResolveSqliteConnectionString(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                sqliteConnectionString,
                sqlite => sqlite.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAlbumCatalogRepository, AlbumCatalogRepository>();
        services.AddScoped<IStickerCollectionRepository, StickerCollectionRepository>();
        services.AddScoped<IStickerDuplicateRepository, StickerDuplicateRepository>();
        services.AddScoped<IStickerImageRepository, StickerImageRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IAlbumSeedReader, JsonAlbumSeedReader>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    private static string ResolveSqliteConnectionString(IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=App_Data/panini2026-tracker.db";

        var builder = new SqliteConnectionStringBuilder(rawConnectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            dataSource = Path.Combine("App_Data", "panini2026-tracker.db");
        }

        var absolutePath = Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, dataSource));

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        builder.DataSource = absolutePath;
        return builder.ToString();
    }
}
