namespace ChessFederationManager.Domain.Entities;

public sealed class Registration
{
    public Guid CompetitionId { get; }
    public Guid PlayerId { get; }
    public DateTimeOffset RegisteredAtUtc { get; }

    public Registration(Guid competitionId, Guid playerId, DateTimeOffset registeredAtUtc)
    {
        if (competitionId == Guid.Empty) throw new ArgumentException("CompetitionId is required.");
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.");

        CompetitionId = competitionId;
        PlayerId = playerId;
        RegisteredAtUtc = registeredAtUtc;
    }
}
