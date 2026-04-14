using CulinaryPairing.Bricks.Model;

namespace CulinaryPairing.Domain.Dishes.Events;

public record DishRenamed(Dish Dish, string OldName, string NewName) : IDomainEvent;