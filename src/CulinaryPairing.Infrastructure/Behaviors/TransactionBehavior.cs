using System.Transactions;
using CulinaryPairing.Infrastructure.Helpers;
using Mediator;

namespace CulinaryPairing.Infrastructure.Behaviors;

public class TransactionBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    static readonly TransactionOptions s_options = new()
    {
        IsolationLevel = IsolationLevel.ReadCommitted,
        Timeout = TransactionManager.MaximumTimeout
    };

    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message.GetType().IsCommand())
        {
            using var scope = new TransactionScope(
                TransactionScopeOption.Required, s_options,
                TransactionScopeAsyncFlowOption.Enabled);
            var response = await next(message, cancellationToken);
            scope.Complete();
            return response;
        }

        return await next(message, cancellationToken);
    }
}