using System.Reflection;
using CulinaryPairing.Application;
using CulinaryPairing.Domain.Dishes;
using CulinaryPairing.Domain.Pairings;
using CulinaryPairing.Domain.Users;
using CulinaryPairing.Infrastructure.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CulinaryPairing.Infrastructure.Database;

public class ApplicationDbContext : IdentityDbContext<CulinaryUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Dish> Dishes { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
    public DbSet<Pairing> Pairings { get; set; } = null!;
    public DbSet<OutboxMessage> Outbox { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}