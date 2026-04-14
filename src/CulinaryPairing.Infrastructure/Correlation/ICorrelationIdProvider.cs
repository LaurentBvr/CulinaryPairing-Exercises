namespace CulinaryPairing.Infrastructure.Correlation;

public interface ICorrelationIdProvider
{
    string CorrelationId { get; }
}