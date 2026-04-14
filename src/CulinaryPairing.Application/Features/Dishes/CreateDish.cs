using Ardalis.Result;
using FluentValidation;
using Mediator;
using CulinaryPairing.Domain.Dishes;

namespace CulinaryPairing.Application.Features.Dishes;

public record CreateDish(Guid DishId, string Name) : ICommand<Result>;

public class CreateDishValidator : AbstractValidator<CreateDish>
{
    public CreateDishValidator()
    {
        RuleFor(x => x.DishId).NotEmpty().WithMessage("L'identifiant du plat est requis");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom est requis")
            .Length(1, 150).WithMessage("Le nom doit contenir entre 1 et 150 caracteres");
    }
}

public class CreateDishHandler(IApplicationDbContext context)
    : ICommandHandler<CreateDish, Result>
{
    public async ValueTask<Result> Handle(CreateDish command, CancellationToken cancellationToken)
    {
        var dish = new Dish(command.DishId, command.Name);
        await context.Dishes.AddAsync(dish, cancellationToken);
        return Result.Success();
    }
}