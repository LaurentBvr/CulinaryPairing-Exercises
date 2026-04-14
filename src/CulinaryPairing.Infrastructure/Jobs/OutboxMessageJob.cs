using System.Reflection;
using System.Text.Json;
using CulinaryPairing.Bricks.Model;
using CulinaryPairing.Infrastructure.Database;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace CulinaryPairing.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class OutboxMessageJob(
    ApplicationDbContext dbContext,
    IPublisher publisher,
    TimeProvider timeProvider) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await dbContext.Outbox
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync();

        foreach (var message in messages)
        {
            try
            {
                var type = Assembly.Load("CulinaryPairing.Domain")
                    .GetType(message.Type);

                if (type is null)
                {
                    message.Error = $"Type '{message.Type}' not found";
                    message.ProcessedOn = timeProvider.GetUtcNow();
                    continue;
                }

                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(
                    message.Content, type)!;

                await publisher.Publish(domainEvent);
                message.ProcessedOn = timeProvider.GetUtcNow();
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
            }
        }

        await dbContext.SaveChangesAsync();
    }
}