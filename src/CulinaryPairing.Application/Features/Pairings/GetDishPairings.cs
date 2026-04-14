using Ardalis.Result;
using Mediator;
using Microsoft.EntityFrameworkCore;
using CulinaryPairing.Domain.Pairings;

namespace CulinaryPairing.Application.Features.Pairings;

public record GetDishPairings(Guid DishId) : IQuery<Result<IReadOnlyList<PairingHeader>>>;

public record PairingHeader(Guid Id, string BeverageName, PairingScore Score, bool IsValidated);

public class GetDishPairingsHandler(IApplicationDbContext context)
    : IQueryHandler<GetDishPairings, Result<IReadOnlyList<PairingHeader>>>
{
    public async ValueTask<Result<IReadOnlyList<PairingHeader>>> Handle(
        GetDishPairings query, CancellationToken cancellationToken)
    {
        var pairings = await context.Pairings
            .AsNoTracking()
            .Where(p => p.DishId == query.DishId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PairingHeader(p.Id, p.BeverageName, p.Score, p.IsValidated))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<PairingHeader>>(pairings);
    }
}