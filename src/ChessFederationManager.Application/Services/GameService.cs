using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Domain.Enums;

namespace ChessFederationManager.Application.Services;

public sealed class GameService
{
    private readonly IGameRepository _games;
    private readonly ICompetitionRepository _competitions;
    private readonly IPlayerRepository _players;

    private const int EloKFactor = 32;

    public GameService(IGameRepository games, ICompetitionRepository competitions, IPlayerRepository players)
    {
        _games = games;
        _competitions = competitions;
        _players = players;
    }

    public Task<IReadOnlyList<Game>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken ct = default)
        => _games.GetByCompetitionIdAsync(competitionId, ct);

    public async Task<Game> CreateGameAsync(Guid competitionId, Guid whitePlayerId, Guid blackPlayerId, CancellationToken ct = default)
    {
        var comp = await _competitions.GetByIdAsync(competitionId, ct) ?? throw new InvalidOperationException("Competition not found.");
        if (!comp.IsPlayerRegistered(whitePlayerId) || !comp.IsPlayerRegistered(blackPlayerId))
            throw new InvalidOperationException("Both players must be registered in the competition.");

        var white = await _players.GetByIdAsync(whitePlayerId, ct) ?? throw new InvalidOperationException("White player not found.");
        var black = await _players.GetByIdAsync(blackPlayerId, ct) ?? throw new InvalidOperationException("Black player not found.");
        if (white.Id == black.Id) throw new InvalidOperationException("Players must be different.");

        var game = new Game(competitionId, whitePlayerId, blackPlayerId);
        await _games.AddAsync(game, ct);
        return game;
    }

    public async Task AddMoveAsync(Guid gameId, int ply, string notation, CancellationToken ct = default)
    {
        var game = await _games.GetByIdAsync(gameId, ct) ?? throw new InvalidOperationException("Game not found.");
        if (game.Result != GameResult.NotPlayed)
            throw new InvalidOperationException("Game is finished.");

        game.AddMove(new Move(ply, notation));
        await _games.UpdateAsync(game, ct);
    }

    public async Task SetResultAsync(Guid gameId, GameResult result, CancellationToken ct = default)
    {
        var game = await _games.GetByIdAsync(gameId, ct) ?? throw new InvalidOperationException("Game not found.");
        if (game.Result != GameResult.NotPlayed)
            throw new InvalidOperationException("Result already recorded.");

        var white = await _players.GetByIdAsync(game.WhitePlayerId, ct) ?? throw new InvalidOperationException("White player not found.");
        var black = await _players.GetByIdAsync(game.BlackPlayerId, ct) ?? throw new InvalidOperationException("Black player not found.");

        var (whiteScore, blackScore) = result switch
        {
            GameResult.WhiteWin => (1d, 0d),
            GameResult.BlackWin => (0d, 1d),
            GameResult.Draw => (0.5d, 0.5d),
            _ => throw new InvalidOperationException("Invalid result.")
        };

        var expectedWhite = ExpectedScore(white.Elo, black.Elo);
        var expectedBlack = ExpectedScore(black.Elo, white.Elo);

        var newWhite = CalculateElo(white.Elo, whiteScore, expectedWhite);
        var newBlack = CalculateElo(black.Elo, blackScore, expectedBlack);

        white.SetElo(newWhite);
        black.SetElo(newBlack);

        game.SetResult(result);

        await _players.UpdateAsync(white, ct);
        await _players.UpdateAsync(black, ct);
        await _games.UpdateAsync(game, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _games.DeleteAsync(id, ct);

    private static double ExpectedScore(int playerElo, int opponentElo)
    {
        var exponent = (opponentElo - playerElo) / 400.0;
        return 1.0 / (1.0 + Math.Pow(10, exponent));
    }

    private static int CalculateElo(int current, double actualScore, double expectedScore)
    {
        var delta = EloKFactor * (actualScore - expectedScore);
        var updated = current + delta;
        var rounded = (int)Math.Round(updated, MidpointRounding.AwayFromZero);
        return Math.Max(0, rounded);
    }
}
