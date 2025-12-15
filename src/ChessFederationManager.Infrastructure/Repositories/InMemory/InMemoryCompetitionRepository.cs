using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Infrastructure.Repositories.InMemory;

public sealed class InMemoryCompetitionRepository : ICompetitionRepository
{
    private readonly List<Competition> _competitions = new();

    public Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<Competition>)_competitions.ToList());

    public Task<Competition?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_competitions.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Competition competition, CancellationToken ct = default)
    {
        _competitions.Add(competition);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Competition competition, CancellationToken ct = default)
    {
        // In-memory: même logique que Player -> rien si référence conservée.
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _competitions.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}
