using Ardalis.Result;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CulinaryPairing.Application.Features.Dishes;

public record GetDishDetail(Guid DishId) : IQuery<Result<DishDetailModel>>;

public record DishDetailModel(
    Guid Id, string Name, string? Description,
    DateTime CreatedAt, IReadOnlyList<IngredientModel> Ingredients);

public record IngredientModel(Guid Id, string Name, string Category);

public class GetDishDetailHandler(IApplicationDbContext context)
    : IQueryHandler<GetDishDetail, Result<DishDetailModel>>
{
    public async ValueTask<Result<DishDetailModel>> Handle(
        GetDishDetail request, CancellationToken cancellationToken)
    {
        var dish = await context.Dishes
            .AsNoTracking()
            .Include(d => d.Ingredients)
            .FirstOrDefaultAsync(d => d.Id == request.DishId, cancellationToken);

        if (dish is null)
            return Result.NotFound();

        return Result.Success(new DishDetailModel(
            dish.Id, dish.Name, dish.Description, dish.CreatedAt,
            dish.Ingredients.Select(i => new IngredientModel(
                i.Id, i.Name, i.Category.ToString())).ToList()));
    }
}