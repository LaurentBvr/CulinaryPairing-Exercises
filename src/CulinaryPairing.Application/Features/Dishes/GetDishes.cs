using Ardalis.Result;
using Mediator;
using Microsoft.EntityFrameworkCore;
using CulinaryPairing.Domain.Dishes;

namespace CulinaryPairing.Application.Features.Dishes;

public record GetDishes : IQuery<Result<IReadOnlyList<DishHeader>>>;

public record DishHeader(Guid Id, string Name, int IngredientCount);

public class GetDishesHandler(IApplicationDbContext context)
    : IQueryHandler<GetDishes, Result<IReadOnlyList<DishHeader>>>
{
    public async ValueTask<Result<IReadOnlyList<DishHeader>>> Handle(
        GetDishes query, CancellationToken cancellationToken)
    {
        var dishes = await context.Dishes
            .AsNoTracking()
            .Include(d => d.Ingredients)
            .OrderBy(d => d.Name)
            .Select(d => new DishHeader(d.Id, d.Name, d.Ingredients.Count()))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<DishHeader>>(dishes);
    }
}