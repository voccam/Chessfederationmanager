using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Application.Abstractions;

public interface IGameRepository
{
    Task<IReadOnlyList<Game>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken ct = default);
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Game game, CancellationToken ct = default);
    Task UpdateAsync(Game game, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
