namespace ChessFederationManager.Domain.Entities;

public sealed class Move
{
    public int Ply { get; private set; } // 1 = premier demi-coup, 2 = second demi-coup, etc.
    public string Notation { get; }
    public DateTimeOffset PlayedAtUtc { get; }

    public Move(int ply, string notation, DateTimeOffset? playedAtUtc = null)
    {
        if (ply <= 0) throw new ArgumentException("Ply must be >= 1.");
        if (string.IsNullOrWhiteSpace(notation)) throw new ArgumentException("Notation is required.");

        Ply = ply;
        Notation = notation.Trim();
        PlayedAtUtc = playedAtUtc ?? DateTimeOffset.UtcNow;
    }
}
