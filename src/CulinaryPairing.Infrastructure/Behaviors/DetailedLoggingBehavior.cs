using System.Text.Json;
using Mediator;
using Microsoft.FeatureManagement;
using Serilog;

namespace CulinaryPairing.Infrastructure.Behaviors;

public class DetailedLoggingBehavior<TMessage, TResponse>(
    IFeatureManager featureManager)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (await featureManager.IsEnabledAsync("DetailedLogging"))
        {
            var json = JsonSerializer.Serialize(message);
            Log.Information("Request details: {RequestType} {Details}",
                message.GetType().Name, json);
        }

        return await next(message, cancellationToken);
    }
}