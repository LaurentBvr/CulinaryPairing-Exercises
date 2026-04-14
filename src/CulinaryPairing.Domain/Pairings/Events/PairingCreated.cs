using CulinaryPairing.Bricks.Model;

namespace CulinaryPairing.Domain.Pairings.Events;

public record PairingCreated(Pairing Pairing) : IDomainEvent;