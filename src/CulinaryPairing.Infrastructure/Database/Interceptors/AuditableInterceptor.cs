using CulinaryPairing.Bricks.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CulinaryPairing.Infrastructure.Database.Interceptors;

public class AuditableInterceptor(TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditInfo(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditInfo(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateAuditInfo(DbContext? context)
    {
        if (context is null) return;
        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            var now = timeProvider.GetUtcNow();

            if (entry.State == EntityState.Added)
            {
                entry.Entity.Audit.Created = now;
                entry.Entity.Audit.CreatedBy = "system";
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.Audit.Modified = now;
                entry.Entity.Audit.ModifiedBy = "system";
            }
        }
    }
}