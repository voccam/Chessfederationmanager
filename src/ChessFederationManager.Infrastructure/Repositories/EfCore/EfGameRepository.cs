using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChessFederationManager.Infrastructure.Repositories.EfCore;

public sealed class EfGameRepository : IGameRepository
{
    private readonly ChessDbContext _db;

    public EfGameRepository(ChessDbContext db) => _db = db;

    public async Task<IReadOnlyList<Game>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken ct = default)
    {
        var records = await _db.Games.AsNoTracking()
            .Include(g => g.Moves)
            .Where(g => g.CompetitionId == competitionId)
            .ToListAsync(ct);

        return records.Select(r => r.ToDomain()).ToList();
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.Games.AsNoTracking()
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        return r?.ToDomain();
    }

    public async Task AddAsync(Game game, CancellationToken ct = default)
    {
        var rec = game.ToRecord();

        // Important : assigner GameId aux moves
        foreach (var m in rec.Moves)
        {
            m.GameId = rec.Id;
            if (m.Id == Guid.Empty) m.Id = Guid.NewGuid();
        }

        _db.Games.Add(rec);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        var rec = await _db.Games
            .Include(g => g.Moves)
            .FirstOrDefaultAsync(g => g.Id == game.Id, ct)
            ?? throw new InvalidOperationException("Game not found.");

        rec.Result = game.Result;

        // Stratégie simple : on remplace tous les coups
        _db.Moves.RemoveRange(rec.Moves);
        rec.Moves = game.Moves.Select(m => new MoveRecord
        {
            Id = Guid.NewGuid(),
            GameId = rec.Id,
            Ply = m.Ply,
            Notation = m.Notation,
            PlayedAtUtc = m.PlayedAtUtc
        }).ToList();

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rec = await _db.Games.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (rec is null) return;

        _db.Games.Remove(rec);
        await _db.SaveChangesAsync(ct);
    }
}
