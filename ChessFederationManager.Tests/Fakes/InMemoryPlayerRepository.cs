using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Tests.Fakes;

public sealed class InMemoryPlayerRepository : IPlayerRepository
{
    private readonly List<Player> _players = new();

    public Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<Player>)_players.ToList());

    public Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_players.FirstOrDefault(p => p.Id == id));

    public Task<Player?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult(_players.FirstOrDefault(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Player player, CancellationToken ct = default) { _players.Add(player); return Task.CompletedTask; }
    public Task UpdateAsync(Player player, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Guid id, CancellationToken ct = default) { _players.RemoveAll(p => p.Id == id); return Task.CompletedTask; }
}
