using CulinaryPairing.Domain.Pairings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryPairing.Infrastructure.Database.Configurations;

public class PairingConfiguration : IEntityTypeConfiguration<Pairing>
{
    public void Configure(EntityTypeBuilder<Pairing> builder)
    {
        builder.ToTable("Pairings");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.BeverageName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.Score)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.IsValidated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasIndex(p => p.DishId);
        builder.HasIndex(p => p.IsValidated);
    }
}