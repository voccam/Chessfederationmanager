using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Application.Services;

public sealed class PlayerService
{
    private readonly IPlayerRepository _players;

    public PlayerService(IPlayerRepository players)
    {
        _players = players;
    }

    public Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default)
        => _players.GetAllAsync(ct);

    public async Task<Player> CreateAsync(string firstName, string lastName, string email, int elo = 1200, CancellationToken ct = default)
    {
        var existing = await _players.GetByEmailAsync(email.Trim(), ct);
        if (existing is not null)
            throw new InvalidOperationException("A player with this email already exists.");

        var player = new Player(firstName, lastName, email, elo);
        await _players.AddAsync(player, ct);
        return player;
    }

    public async Task UpdateAsync(Guid id, string firstName, string lastName, string email, CancellationToken ct = default)
    {
        var player = await _players.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Player not found.");

        var other = await _players.GetByEmailAsync(email.Trim(), ct);
        if (other is not null && other.Id != id)
            throw new InvalidOperationException("Another player already uses this email.");

        player.UpdateIdentity(firstName, lastName, email);
        await _players.UpdateAsync(player, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _players.DeleteAsync(id, ct);
}
