using Microsoft.EntityFrameworkCore;
using Panini2026Tracker.Domain.Entities;

namespace Panini2026Tracker.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<StickerCatalogItem> StickerCatalogItems => Set<StickerCatalogItem>();
    public DbSet<StickerCollectionEntry> StickerCollectionEntries => Set<StickerCollectionEntry>();
    public DbSet<StickerDuplicateEntry> StickerDuplicateEntries => Set<StickerDuplicateEntry>();
    public DbSet<StickerImage> StickerImages => Set<StickerImage>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(builder =>
        {
            builder.ToTable("Countries");
            builder.HasKey(country => country.Id);
            builder.Property(country => country.Code).HasMaxLength(10).IsRequired();
            builder.Property(country => country.Name).HasMaxLength(120).IsRequired();
            builder.Property(country => country.FlagCode).HasColumnName("FlagEmoji").HasMaxLength(16).IsRequired();
            builder.HasIndex(country => country.Code).IsUnique();
        });

        modelBuilder.Entity<StickerCatalogItem>(builder =>
        {
            builder.ToTable("StickerCatalogItems");
            builder.HasKey(sticker => sticker.Id);
            builder.Property(sticker => sticker.StickerCode).HasMaxLength(40).IsRequired();
            builder.Property(sticker => sticker.DisplayName).HasMaxLength(160).IsRequired();
            builder.Property(sticker => sticker.Type).HasMaxLength(40).IsRequired();
            builder.Property(sticker => sticker.ImageReference).HasMaxLength(260);
            builder.HasIndex(sticker => sticker.StickerCode).IsUnique();
            builder.HasOne(sticker => sticker.Country)
                .WithMany(country => country.Stickers)
                .HasForeignKey(sticker => sticker.CountryId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        modelBuilder.Entity<StickerCollectionEntry>(builder =>
        {
            builder.ToTable("StickerCollectionEntries");
            builder.HasKey(entry => entry.Id);
            builder.Property(entry => entry.Notes).HasMaxLength(1000);
            builder.HasIndex(entry => entry.StickerCatalogItemId).IsUnique();
            builder.HasOne(entry => entry.StickerCatalogItem)
                .WithOne(sticker => sticker.CollectionEntry)
                .HasForeignKey<StickerCollectionEntry>(entry => entry.StickerCatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StickerDuplicateEntry>(builder =>
        {
            builder.ToTable("StickerDuplicateEntries");
            builder.HasKey(entry => entry.Id);
            builder.HasIndex(entry => entry.StickerCatalogItemId).IsUnique();
            builder.HasOne(entry => entry.StickerCatalogItem)
                .WithOne(sticker => sticker.DuplicateEntry)
                .HasForeignKey<StickerDuplicateEntry>(entry => entry.StickerCatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StickerImage>(builder =>
        {
            builder.ToTable("StickerImages");
            builder.HasKey(image => image.Id);
            builder.Property(image => image.RelativePath).HasMaxLength(260).IsRequired();
            builder.Property(image => image.OriginalFileName).HasMaxLength(260).IsRequired();
            builder.Property(image => image.ContentType).HasMaxLength(120).IsRequired();
            builder.HasIndex(image => image.StickerCatalogItemId).IsUnique();
            builder.HasOne(image => image.StickerCatalogItem)
                .WithOne(sticker => sticker.StickerImage)
                .HasForeignKey<StickerImage>(image => image.StickerCatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SystemLog>(builder =>
        {
            builder.ToTable("SystemLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.Category).HasMaxLength(80).IsRequired();
            builder.Property(log => log.Action).HasMaxLength(120).IsRequired();
            builder.Property(log => log.Level).HasMaxLength(20).IsRequired();
            builder.Property(log => log.Details).HasMaxLength(1000).IsRequired();
        });
    }
}
