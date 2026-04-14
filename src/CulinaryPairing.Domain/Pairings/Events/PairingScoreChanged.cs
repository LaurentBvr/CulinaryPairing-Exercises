using CulinaryPairing.Bricks.Model;

namespace CulinaryPairing.Domain.Pairings.Events;

public record PairingScoreChanged(Pairing Pairing, PairingScore OldScore, PairingScore NewScore) : IDomainEvent;