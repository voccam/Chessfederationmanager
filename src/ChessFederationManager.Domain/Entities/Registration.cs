namespace ChessFederationManager.Domain.Entities;

/// <summary>
/// Represents the fact that a player is registered in a competition.
/// </summary>
public class Registration
{
    public Guid Id { get; }
    public Guid CompetitionId { get; }
    public Player Player { get; }
    public DateTime RegisteredAt { get; }
    public decimal FeeAmount { get; }
    public bool FeePaid { get; private set; }
    public int? Seed { get; private set; }

    public Registration(Guid competitionId, Player player, decimal feeAmount, DateTime? registeredAt = null, bool feePaid = false)
    {
        if (competitionId == Guid.Empty)
        {
            throw new ArgumentException("Competition id must be set.", nameof(competitionId));
        }

        Player = player ?? throw new ArgumentNullException(nameof(player));
        if (feeAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(feeAmount), "Fee cannot be negative.");
        }

        Id = Guid.NewGuid();
        CompetitionId = competitionId;
        FeeAmount = feeAmount;
        FeePaid = feePaid;
        RegisteredAt = registeredAt ?? DateTime.UtcNow;
    }

    public void MarkFeeAsPaid() => FeePaid = true;

    public void AssignSeed(int seed)
    {
        if (seed < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(seed), "Seed must be positive.");
        }

        Seed = seed;
    }
}
