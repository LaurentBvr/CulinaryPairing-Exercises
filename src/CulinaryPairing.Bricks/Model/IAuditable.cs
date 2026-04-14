namespace CulinaryPairing.Bricks.Model;

public interface IAuditable
{
    AuditInfo Audit { get; }
}