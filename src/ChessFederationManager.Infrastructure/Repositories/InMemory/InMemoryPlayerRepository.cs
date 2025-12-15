using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Infrastructure.Repositories.InMemory;

public sealed class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly List<Player> _players = new();

    public Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<Player>)_players.ToList());

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_players.FirstOrDefault(p => p.Id == id));

    public Task<Player?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult(_players.FirstOrDefault(p =>
            p.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Player player, CancellationToken ct = default)
    {
        _players.Add(player);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Player player, CancellationToken ct = default)
    {
        // In-memory: rien à faire si tu gardes la même instance.
        // Si un jour tu clones les objets, tu devras remplacer l'élément de la liste.
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _players.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
