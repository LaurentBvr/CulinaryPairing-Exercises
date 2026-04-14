using CulinaryPairing.Bricks.Model;

namespace CulinaryPairing.Domain.Dishes.Events;

public record DishCreated(Dish Dish) : IDomainEvent;