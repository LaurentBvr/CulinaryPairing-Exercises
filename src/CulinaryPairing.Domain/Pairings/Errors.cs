using Ardalis.Result;

namespace CulinaryPairing.Domain.Pairings;

public static class Errors
{
    public static Result AlreadyValidated(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Cet accord est deja valide",
            "Pairings.AlreadyValidated",
            ValidationSeverity.Error));

    public static Result NotValidated(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Cet accord n'est pas valide",
            "Pairings.NotValidated",
            ValidationSeverity.Error));

    public static Result CannotChangeScoreWhenValidated(Guid id) =>
        Result.Invalid(new ValidationError(
            id.ToString(),
            "Impossible de changer le score d'un accord valide",
            "Pairings.CannotChangeScore",
            ValidationSeverity.Error));
}