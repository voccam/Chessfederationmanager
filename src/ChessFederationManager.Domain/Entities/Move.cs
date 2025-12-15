namespace ChessFederationManager.Domain.Entities;

/// <summary>
/// Represents a single move in a chess game using SAN notation.
/// </summary>
public class Move
{
    public int Number { get; }
    public string Notation { get; }
    public TimeSpan? TimeSpent { get; }
    public string? Comment { get; }
    public DateTime RecordedAt { get; }

    public Move(int number, string notation, TimeSpan? timeSpent = null, string? comment = null, DateTime? recordedAt = null)
    {
        if (number < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Move number must be positive.");
        }

        if (string.IsNullOrWhiteSpace(notation))
        {
            throw new ArgumentException("Move notation must be provided.", nameof(notation));
        }

        Number = number;
        Notation = notation.Trim();
        TimeSpent = timeSpent;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        RecordedAt = recordedAt ?? DateTime.UtcNow;
    }

    public bool EndsWithCheck => Notation.Contains('+', StringComparison.Ordinal);
    public bool EndsWithMate => Notation.Contains('#', StringComparison.Ordinal);

    public override string ToString() => $"{Number}. {Notation}";
}
