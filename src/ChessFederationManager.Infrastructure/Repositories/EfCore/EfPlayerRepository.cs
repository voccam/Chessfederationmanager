using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChessFederationManager.Infrastructure.Repositories.EfCore;

public sealed class EfPlayerRepository : IPlayerRepository
{
    private readonly ChessDbContext _db;

    public EfPlayerRepository(ChessDbContext db) => _db = db;

    public async Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default)
        => await _db.Players.AsNoTracking()
            .Select(p => p.ToDomain())
            .ToListAsync(ct);

    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return r?.ToDomain();
    }

    public async Task<Player?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var e = email.Trim();
        var r = await _db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Email == e, ct);
        return r?.ToDomain();
    }

    public async Task AddAsync(Player player, CancellationToken ct = default)
    {
        _db.Players.Add(player.ToRecord());
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Player player, CancellationToken ct = default)
    {
        var rec = await _db.Players.FirstOrDefaultAsync(p => p.Id == player.Id, ct)
                  ?? throw new InvalidOperationException("Player not found.");

        rec.FirstName = player.FirstName;
        rec.LastName = player.LastName;
        rec.Email = player.Email;
        rec.Elo = player.Elo;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rec = await _db.Players.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (rec is null) return;

        _db.Players.Remove(rec);
        await _db.SaveChangesAsync(ct);
    }
}
