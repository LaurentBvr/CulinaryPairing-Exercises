using CulinaryPairing.Domain.Dishes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryPairing.Infrastructure.Database.Configurations;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("Dishes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.HasMany(d => d.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.DishId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Ingredients)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}