using ChessFederationManager.Application.Abstractions;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChessFederationManager.Infrastructure.Repositories.EfCore;

public sealed class EfCompetitionRepository : ICompetitionRepository
{
    private readonly ChessDbContext _db;

    public EfCompetitionRepository(ChessDbContext db) => _db = db;

    public async Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken ct = default)
    {
        var records = await _db.Competitions.AsNoTracking()
            .Include(c => c.Registrations)
            .ToListAsync(ct);

        return records.Select(r => r.ToDomain()).ToList();
    }

    public async Task<Competition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.Competitions.AsNoTracking()
            .Include(c => c.Registrations)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return r?.ToDomain();
    }

    public async Task AddAsync(Competition competition, CancellationToken ct = default)
    {
        _db.Competitions.Add(competition.ToRecord());

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Competition competition, CancellationToken ct = default)
    {
        var rec = await _db.Competitions
            .Include(c => c.Registrations)
            .FirstOrDefaultAsync(c => c.Id == competition.Id, ct)
                  ?? throw new InvalidOperationException("Competition not found.");

        rec.Name = competition.Name;
        rec.StartDate = competition.StartDate;
        rec.Location = competition.Location;
        _db.Registrations.RemoveRange(rec.Registrations);
        rec.Registrations = competition.Registrations.Select(r => r.ToRecord()).ToList();

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var rec = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (rec is null) return;

        _db.Competitions.Remove(rec);
        await _db.SaveChangesAsync(ct);
    }
}
