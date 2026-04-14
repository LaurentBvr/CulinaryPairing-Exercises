using CulinaryPairing.Bricks.Model;

namespace CulinaryPairing.Domain.Dishes.Events;

public record IngredientAdded(Dish Dish, Ingredient Ingredient) : IDomainEvent;