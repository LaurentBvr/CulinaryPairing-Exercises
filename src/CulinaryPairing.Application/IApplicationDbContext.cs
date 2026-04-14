using CulinaryPairing.Domain.Dishes;
using CulinaryPairing.Domain.Pairings;
using Microsoft.EntityFrameworkCore;

namespace CulinaryPairing.Application;

public interface IApplicationDbContext
{
    DbSet<Dish> Dishes { get; }
    DbSet<Ingredient> Ingredients { get; }
    DbSet<Pairing> Pairings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}