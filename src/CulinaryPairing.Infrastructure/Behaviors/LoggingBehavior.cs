using System.Diagnostics;
using Mediator;
using Serilog;
using Serilog.Context;
using Serilog.Core;

namespace CulinaryPairing.Infrastructure.Behaviors;

public class LoggingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var log = Log.ForContext(Constants.SourceContextPropertyName, message.GetType().FullName);

        using (LogContext.PushProperty("RequestName", message.GetType().Name))
        {
            var start = Stopwatch.GetTimestamp();
            TResponse response;
            try
            {
                response = await next(message, cancellationToken);
            }
            catch (Exception ex)
            {
                var errorElapsed = GetElapsedMs(start, Stopwatch.GetTimestamp());
                log.Error(ex, "Request {RequestName} FAILED in {Elapsed:0.00} ms",
                    message.GetType().Name, errorElapsed);
                throw;
            }

            var elapsedMs = GetElapsedMs(start, Stopwatch.GetTimestamp());
            log.Information("Request {RequestName} completed in {Elapsed:0.00} ms",
                message.GetType().Name, elapsedMs);
            return response;
        }
    }

    static double GetElapsedMs(long start, long stop) =>
        Math.Round((stop - start) * 1000 / (double)Stopwatch.Frequency, 2);
}