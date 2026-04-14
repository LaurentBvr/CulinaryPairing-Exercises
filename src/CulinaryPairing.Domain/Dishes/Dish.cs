using Ardalis.Result;
using CulinaryPairing.Bricks.Model;
using CulinaryPairing.Domain.Dishes.Events;

namespace CulinaryPairing.Domain.Dishes;

public class Dish : BaseEntity, IAuditable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public AuditInfo Audit { get; set; } = new();

    private readonly List<Ingredient> _ingredients = new();
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.AsReadOnly();

    private Dish() { Name = null!; }

    public Dish(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Le nom du plat ne peut pas etre vide.", nameof(name));

        Id = id;
        Name = name;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new DishCreated(this));
    }

    public Result SetDescription(string? description)
    {
        if (description is not null && description.Length > 500)
            return Result.Invalid(new ValidationError(
                "Description", "La description ne peut pas depasser 500 caracteres"));

        Description = description;
        return Result.Success();
    }

    public Result AddIngredient(Ingredient ingredient)
    {
        if (ingredient is null)
            return Result.Invalid(new ValidationError(
                "Ingredient", "L'ingredient ne peut pas etre null"));

        if (_ingredients.Any(i => i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase)))
            return Result.Invalid(new ValidationError(
                "Ingredient", $"L'ingredient '{ingredient.Name}' existe deja dans ce plat"));

        _ingredients.Add(ingredient);
        AddDomainEvent(new IngredientAdded(this, ingredient));
        return Result.Success();
    }

    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Invalid(new ValidationError(
                "Name", "Le nouveau nom ne peut pas etre vide"));

        if (newName.Length > 150)
            return Result.Invalid(new ValidationError(
                "Name", "Le nouveau nom ne peut pas depasser 150 caracteres"));

        var oldName = Name;
        Name = newName;
        AddDomainEvent(new DishRenamed(this, oldName, newName));
        return Result.Success();
    }
}