using ChessFederationManager.Domain.Enums;

namespace ChessFederationManager.Domain.Entities;

public sealed class Game
{
    public Guid Id { get; private set; }
    public Guid CompetitionId { get; }
    public Guid WhitePlayerId { get; }
    public Guid BlackPlayerId { get; }

    public GameResult Result { get; private set; } = GameResult.NotPlayed;

    private readonly List<Move> _moves = new();
    public IReadOnlyList<Move> Moves => _moves.AsReadOnly();

    public Game(Guid competitionId, Guid whitePlayerId, Guid blackPlayerId)
    {
        if (competitionId == Guid.Empty) throw new ArgumentException("CompetitionId is required.");
        if (whitePlayerId == Guid.Empty) throw new ArgumentException("WhitePlayerId is required.");
        if (blackPlayerId == Guid.Empty) throw new ArgumentException("BlackPlayerId is required.");
        if (whitePlayerId == blackPlayerId) throw new ArgumentException("Players must be different.");

        Id = Guid.NewGuid();
        CompetitionId = competitionId;
        WhitePlayerId = whitePlayerId;
        BlackPlayerId = blackPlayerId;
    }

    public Game(Guid id, Guid competitionId, Guid whitePlayerId, Guid blackPlayerId)
        : this(competitionId, whitePlayerId, blackPlayerId)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.");
        Id = id;
    }

    public void AddMove(Move move)
    {
        if (move is null) throw new ArgumentNullException(nameof(move));

        // règle métier: les plys doivent être strictement croissants
        if (_moves.Count > 0 && move.Ply <= _moves[^1].Ply)
            throw new InvalidOperationException("Move ply must be strictly increasing.");

        _moves.Add(move);
    }

    public void SetResult(GameResult result)
    {
        Result = result;
    }

    public void LoadFromPersistence(IEnumerable<Move> moves, GameResult result)
    {
       _moves.Clear();
        _moves.AddRange(moves.OrderBy(m => m.Ply));
        Result = result;
    }
}