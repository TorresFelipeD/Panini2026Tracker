using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260420190000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Countries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Countries", x => x.Id));

        migrationBuilder.CreateTable(
            name: "SystemLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Category = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                Action = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_SystemLogs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "StickerCatalogItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                StickerCode = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                Type = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                ImageReference = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                AdditionalInfoJson = table.Column<string>(type: "TEXT", nullable: true),
                MetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                IsProvisional = table.Column<bool>(type: "INTEGER", nullable: false),
                DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                CountryId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StickerCatalogItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_StickerCatalogItems_Countries_CountryId",
                    column: x => x.CountryId,
                    principalTable: "Countries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StickerCollectionEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                StickerCatalogItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                IsOwned = table.Column<bool>(type: "INTEGER", nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StickerCollectionEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_StickerCollectionEntries_StickerCatalogItems_StickerCatalogItemId",
                    column: x => x.StickerCatalogItemId,
                    principalTable: "StickerCatalogItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StickerDuplicateEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                StickerCatalogItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StickerDuplicateEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_StickerDuplicateEntries_StickerCatalogItems_StickerCatalogItemId",
                    column: x => x.StickerCatalogItemId,
                    principalTable: "StickerCatalogItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StickerImages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                StickerCatalogItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                RelativePath = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                ContentType = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                UploadedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StickerImages", x => x.Id);
                table.ForeignKey(
                    name: "FK_StickerImages_StickerCatalogItems_StickerCatalogItemId",
                    column: x => x.StickerCatalogItemId,
                    principalTable: "StickerCatalogItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Countries_Code", table: "Countries", column: "Code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_StickerCatalogItems_CountryId", table: "StickerCatalogItems", column: "CountryId");
        migrationBuilder.CreateIndex(name: "IX_StickerCatalogItems_StickerCode", table: "StickerCatalogItems", column: "StickerCode", unique: true);
        migrationBuilder.CreateIndex(name: "IX_StickerCollectionEntries_StickerCatalogItemId", table: "StickerCollectionEntries", column: "StickerCatalogItemId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_StickerDuplicateEntries_StickerCatalogItemId", table: "StickerDuplicateEntries", column: "StickerCatalogItemId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_StickerImages_StickerCatalogItemId", table: "StickerImages", column: "StickerCatalogItemId", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "StickerCollectionEntries");
        migrationBuilder.DropTable(name: "StickerDuplicateEntries");
        migrationBuilder.DropTable(name: "StickerImages");
        migrationBuilder.DropTable(name: "SystemLogs");
        migrationBuilder.DropTable(name: "StickerCatalogItems");
        migrationBuilder.DropTable(name: "Countries");
    }
}
