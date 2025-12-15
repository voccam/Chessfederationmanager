using ChessFederationManager.Domain.Enums;

namespace ChessFederationManager.Domain.Entities;

/// <summary>
/// Represents a single chess game scheduled inside a competition.
/// </summary>
public class Game
{
    private readonly List<Move> _moves = new();

    public Guid Id { get; }
    public Guid CompetitionId { get; }
    public Player WhitePlayer { get; }
    public Player BlackPlayer { get; }
    public int Round { get; }
    public DateTime ScheduledAt { get; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public GameResult Result { get; private set; } = GameResult.Undecided;
    public IReadOnlyList<Move> Moves => _moves.AsReadOnly();
    public bool IsFinished => Result != GameResult.Undecided;

    public Game(Guid competitionId, Player whitePlayer, Player blackPlayer, int round, DateTime scheduledAt)
    {
        if (competitionId == Guid.Empty)
        {
            throw new ArgumentException("Competition id is required.", nameof(competitionId));
        }

        WhitePlayer = whitePlayer ?? throw new ArgumentNullException(nameof(whitePlayer));
        BlackPlayer = blackPlayer ?? throw new ArgumentNullException(nameof(blackPlayer));

        if (whitePlayer.Id == blackPlayer.Id)
        {
            throw new ArgumentException("A player cannot play against themselves.");
        }

        if (round < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(round), "Round must be a positive number.");
        }

        Id = Guid.NewGuid();
        CompetitionId = competitionId;
        Round = round;
        ScheduledAt = scheduledAt;
    }

    public void StartGame(DateTime startTime)
    {
        if (StartedAt.HasValue)
        {
            throw new InvalidOperationException("The game already started.");
        }

        StartedAt = startTime;
    }

    public void CompleteGame(GameResult result, DateTime? endedAt = null)
    {
        if (result == GameResult.Undecided)
        {
            throw new ArgumentException("Result must be a terminal value.", nameof(result));
        }

        if (IsFinished)
        {
            throw new InvalidOperationException("The result was already recorded.");
        }

        Result = result;
        EndedAt = endedAt ?? DateTime.UtcNow;
    }

    public void AddMove(Move move)
    {
        if (move is null)
        {
            throw new ArgumentNullException(nameof(move));
        }

        if (IsFinished)
        {
            throw new InvalidOperationException("Cannot add moves to a finished game.");
        }

        if (_moves.Count > 0 && move.Number < _moves[^1].Number)
        {
            throw new InvalidOperationException("Moves must be added in chronological order.");
        }

        _moves.Add(move);
    }

    public Move AddMove(int number, string notation, TimeSpan? timeSpent = null, string? comment = null)
    {
        var move = new Move(number, notation, timeSpent, comment);
        AddMove(move);
        return move;
    }
}
