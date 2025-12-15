using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Domain.Enums;

namespace ChessFederationManager.Application.Services;

public sealed class GameService
{
    private readonly IGameRepository _games;
    private readonly ICompetitionRepository _competitions;

    public GameService(IGameRepository games, ICompetitionRepository competitions)
    {
        _games = games;
        _competitions = competitions;
    }

    public async Task<Game> CreateGameAsync(Guid competitionId, Guid whitePlayerId, Guid blackPlayerId, CancellationToken ct = default)
    {
        _ = await _competitions.GetByIdAsync(competitionId, ct) ?? throw new InvalidOperationException("Competition not found.");

        var game = new Game(competitionId, whitePlayerId, blackPlayerId);
        await _games.AddAsync(game, ct);
        return game;
    }

    public async Task AddMoveAsync(Guid gameId, int ply, string notation, CancellationToken ct = default)
    {
        var game = await _games.GetByIdAsync(gameId, ct) ?? throw new InvalidOperationException("Game not found.");

        game.AddMove(new Move(ply, notation));
        await _games.UpdateAsync(game, ct);
    }

    public async Task SetResultAsync(Guid gameId, GameResult result, CancellationToken ct = default)
    {
        var game = await _games.GetByIdAsync(gameId, ct) ?? throw new InvalidOperationException("Game not found.");

        game.SetResult(result);
        await _games.UpdateAsync(game, ct);
    }
}
