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
        => await _db.Competitions.AsNoTracking()
            .Select(c => new Competition(c.Id, c.Name, c.StartDate, c.Location))
            .ToListAsync(ct);

    public async Task<Competition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _db.Competitions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return r is null ? null : new Competition(r.Id, r.Name, r.StartDate, r.Location);
    }

    public async Task AddAsync(Competition competition, CancellationToken ct = default)
    {
        _db.Competitions.Add(new CompetitionRecord
        {
            Id = competition.Id,
            Name = competition.Name,
            StartDate = competition.StartDate,
            Location = competition.Location
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Competition competition, CancellationToken ct = default)
    {
        var rec = await _db.Competitions.FirstOrDefaultAsync(c => c.Id == competition.Id, ct)
                  ?? throw new InvalidOperationException("Competition not found.");

        rec.Name = competition.Name;
        rec.StartDate = competition.StartDate;
        rec.Location = competition.Location;

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
