using Microsoft.EntityFrameworkCore.Migrations;

namespace Panini2026Tracker.Infrastructure.Migrations;

public partial class AddCountryFlagEmoji : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FlagEmoji",
            table: "Countries",
            type: "TEXT",
            maxLength: 16,
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FlagEmoji",
            table: "Countries");
    }
}
