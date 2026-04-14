using Ardalis.Result;
using FluentValidation;
using Mediator;
using CulinaryPairing.Domain.Pairings;

namespace CulinaryPairing.Application.Features.Pairings;

public record CreatePairing(
    Guid PairingId, string BeverageName, Guid DishId,
    PairingScore Score = PairingScore.Medium) : ICommand<Result>;

public class CreatePairingValidator : AbstractValidator<CreatePairing>
{
    public CreatePairingValidator()
    {
        RuleFor(x => x.PairingId).NotEmpty();
        RuleFor(x => x.BeverageName).NotEmpty().WithMessage("Le nom de la boisson est requis")
            .Length(1, 200);
        RuleFor(x => x.DishId).NotEmpty();
    }
}

public class CreatePairingHandler(IApplicationDbContext context)
    : ICommandHandler<CreatePairing, Result>
{
    public async ValueTask<Result> Handle(CreatePairing command, CancellationToken cancellationToken)
    {
        var pairing = new Pairing(command.PairingId, command.BeverageName,
            command.DishId, command.Score);
        await context.Pairings.AddAsync(pairing, cancellationToken);
        return Result.Success();
    }
}