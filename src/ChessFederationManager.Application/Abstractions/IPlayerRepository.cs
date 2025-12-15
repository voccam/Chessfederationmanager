using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.Application.Abstractions;

public interface IPlayerRepository
{
    Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default);
    Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Player?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task AddAsync(Player player, CancellationToken ct = default);
    Task UpdateAsync(Player player, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
