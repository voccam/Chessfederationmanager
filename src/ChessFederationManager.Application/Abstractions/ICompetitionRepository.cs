using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Application.Abstractions;

public interface ICompetitionRepository
{
    Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken ct = default);
    Task<Competition?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Competition competition, CancellationToken ct = default);
    Task UpdateAsync(Competition competition, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

