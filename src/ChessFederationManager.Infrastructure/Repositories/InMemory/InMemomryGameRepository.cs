using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Infrastructure.Repositories.InMemory;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly List<Game> _games = new();

    public Task<IReadOnlyList<Game>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<Game>)_games.Where(g => g.CompetitionId == competitionId).ToList());

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_games.FirstOrDefault(g => g.Id == id));

    public Task AddAsync(Game game, CancellationToken ct = default)
    {
        _games.Add(game);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        // idem: pas nécessaire si tu gardes les mêmes instances.
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _games.RemoveAll(g => g.Id == id);
        return Task.CompletedTask;
    }
}
