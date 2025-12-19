using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Application.Services;

public sealed class CompetitionService
{
    private readonly ICompetitionRepository _competitions;
    private readonly IPlayerRepository _players;

    public CompetitionService(ICompetitionRepository competitions, IPlayerRepository players)
    {
        _competitions = competitions;
        _players = players;
    }

    public Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken ct = default)
        => _competitions.GetAllAsync(ct);

    public Task<Competition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _competitions.GetByIdAsync(id, ct);

    public async Task<Competition> CreateAsync(string name, DateOnly startDate, string location, CancellationToken ct = default)
    {
        var comp = new Competition(name, startDate, location);
        await _competitions.AddAsync(comp, ct);
        return comp;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _competitions.DeleteAsync(id, ct);

    public async Task RegisterPlayerAsync(Guid competitionId, Guid playerId, CancellationToken ct = default)
    {
        var comp = await _competitions.GetByIdAsync(competitionId, ct) ?? throw new InvalidOperationException("Competition not found.");
        var player = await _players.GetByIdAsync(playerId, ct) ?? throw new InvalidOperationException("Player not found.");

        comp.Register(player);
        await _competitions.UpdateAsync(comp, ct);
    }

    public async Task UnregisterPlayerAsync(Guid competitionId, Guid playerId, CancellationToken ct = default)
    {
        var comp = await _competitions.GetByIdAsync(competitionId, ct) ?? throw new InvalidOperationException("Competition not found.");
        comp.Unregister(playerId);
        await _competitions.UpdateAsync(comp, ct);
    }

    public async Task<IReadOnlyList<Player>> GetRegisteredPlayersAsync(Guid competitionId, CancellationToken ct = default)
    {
        var comp = await _competitions.GetByIdAsync(competitionId, ct) ?? throw new InvalidOperationException("Competition not found.");
        var players = await _players.GetAllAsync(ct);
        var registeredIds = comp.Registrations.Select(r => r.PlayerId).ToHashSet();

        return players
            .Where(p => registeredIds.Contains(p.Id))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToList();
    }
}
