using Ardalis.Result;
using Mediator;

namespace CulinaryPairing.Application.Features.Pairings;

public record ValidatePairing(Guid PairingId) : ICommand<Result>;

public class ValidatePairingHandler(IApplicationDbContext context)
    : ICommandHandler<ValidatePairing, Result>
{
    public async ValueTask<Result> Handle(ValidatePairing command, CancellationToken cancellationToken)
    {
        var pairing = await context.Pairings.FindAsync(
            [command.PairingId], cancellationToken);

        if (pairing is null)
            return Result.NotFound();

        return pairing.Validate();
    }
}