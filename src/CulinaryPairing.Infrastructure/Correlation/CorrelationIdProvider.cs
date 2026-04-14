using Microsoft.AspNetCore.Http;

namespace CulinaryPairing.Infrastructure.Correlation;

public class CorrelationIdProvider : ICorrelationIdProvider
{
    public string CorrelationId { get; }

    public CorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Headers.TryGetValue("X-Correlation-Id", out var existing) == true
            && !string.IsNullOrWhiteSpace(existing))
        {
            CorrelationId = existing.ToString();
        }
        else
        {
            CorrelationId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        }
    }
}