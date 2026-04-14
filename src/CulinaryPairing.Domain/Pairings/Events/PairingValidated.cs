using CulinaryPairing.Bricks.Model;

namespace CulinaryPairing.Domain.Pairings.Events;

public record PairingValidated(Pairing Pairing) : IDomainEvent;