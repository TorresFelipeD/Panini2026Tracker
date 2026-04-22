using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Panini2026Tracker.Infrastructure.Persistence;

namespace Panini2026Tracker.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

        modelBuilder.Entity("Panini2026Tracker.Domain.Entities.Country", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("TEXT");

                b.Property<string>("Code")
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnType("TEXT");

                b.Property<int>("DisplayOrder")
                    .HasColumnType("INTEGER");

                b.Property<string>("FlagEmoji")
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Code")
                    .IsUnique();

                b.ToTable("Countries");
            });
    }
}
