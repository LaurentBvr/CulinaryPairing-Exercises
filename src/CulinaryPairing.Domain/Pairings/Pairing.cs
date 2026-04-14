using Ardalis.Result;
using CulinaryPairing.Bricks.Model;
using CulinaryPairing.Domain.Pairings.Events;

namespace CulinaryPairing.Domain.Pairings;

public class Pairing : BaseEntity, IAuditable
{
    public Guid Id { get; private set; }
    public string BeverageName { get; private set; }
    public string? Notes { get; private set; }
    public PairingScore Score { get; private set; }
    public bool IsValidated { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ValidatedAt { get; private set; }
    public Guid DishId { get; private set; }
    public AuditInfo Audit { get; set; } = new();

    private Pairing() { BeverageName = null!; }

    public Pairing(Guid id, string beverageName, Guid dishId, PairingScore score = PairingScore.Medium)
    {
        if (string.IsNullOrWhiteSpace(beverageName))
            throw new ArgumentException(
                "Le nom de la boisson est requis.", nameof(beverageName));

        Id = id;
        BeverageName = beverageName;
        DishId = dishId;
        Score = score;
        IsValidated = false;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new PairingCreated(this));
    }

    public Result SetNotes(string? notes)
    {
        if (notes is not null && notes.Length > 1000)
            return Result.Invalid(new ValidationError(
                "Notes", "Les notes ne peuvent pas depasser 1000 caracteres"));

        Notes = notes;
        return Result.Success();
    }

    public Result Validate()
    {
        if (IsValidated)
            return Errors.AlreadyValidated(Id);

        IsValidated = true;
        ValidatedAt = DateTime.UtcNow;
        AddDomainEvent(new PairingValidated(this));
        return Result.Success();
    }

    public Result Invalidate()
    {
        if (!IsValidated)
            return Errors.NotValidated(Id);

        IsValidated = false;
        ValidatedAt = null;
        return Result.Success();
    }

    public Result SetScore(PairingScore score)
    {
        if (IsValidated)
            return Errors.CannotChangeScoreWhenValidated(Id);

        var oldScore = Score;
        Score = score;
        AddDomainEvent(new PairingScoreChanged(this, oldScore, score));
        return Result.Success();
    }
}