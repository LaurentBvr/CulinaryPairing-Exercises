namespace CulinaryPairing.Domain.Dishes;

public class Ingredient
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public IngredientCategory Category { get; private set; }
    public Guid DishId { get; private set; }

    private Ingredient() { Name = null!; }

    public Ingredient(string name, IngredientCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de l'ingredient est requis.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException(
                "Le nom ne peut pas depasser 100 caracteres.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Category = category;
    }
}