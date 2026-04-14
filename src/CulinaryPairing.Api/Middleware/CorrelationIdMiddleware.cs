using CulinaryPairing.Infrastructure.Correlation;
using Serilog.Context;

namespace CulinaryPairing.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationProvider)
    {
        context.Response.Headers["X-Correlation-Id"] = correlationProvider.CorrelationId;

        using (LogContext.PushProperty("CorrelationId", correlationProvider.CorrelationId))
        {
            await _next(context);
        }
    }
}