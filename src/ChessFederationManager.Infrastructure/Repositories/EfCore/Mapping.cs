using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Domain.Enums;
using ChessFederationManager.Infrastructure.Data;

namespace ChessFederationManager.Infrastructure.Repositories.EfCore;

internal static class Mapping
{
    // ---------- Player ----------
    public static PlayerRecord ToRecord(this Player p) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Email = p.Email,
        Elo = p.Elo
    };

    public static Player ToDomain(this PlayerRecord r)
        => new Player(r.Id, r.FirstName, r.LastName, r.Email, r.Elo);

    // ---------- Competition ----------
    public static CompetitionRecord ToRecord(this Competition c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        StartDate = c.StartDate,
        Location = c.Location,
        Registrations = c.Registrations.Select(ToRecord).ToList()
    };

    public static RegistrationRecord ToRecord(this Registration r) => new()
    {
        CompetitionId = r.CompetitionId,
        PlayerId = r.PlayerId,
        RegisteredAtUtc = r.RegisteredAtUtc
    };

    public static Registration ToDomain(this RegistrationRecord r)
        => new Registration(r.CompetitionId, r.PlayerId, r.RegisteredAtUtc);

    public static Competition ToDomain(this CompetitionRecord r)
    {
        var c = new Competition(r.Id, r.Name, r.StartDate, r.Location);
        c.LoadRegistrations(r.Registrations.Select(ToDomain));
        return c;
    }

    // ---------- Game ----------
    public static GameRecord ToRecord(this Game g) => new()
    {
        Id = g.Id,
        CompetitionId = g.CompetitionId,
        WhitePlayerId = g.WhitePlayerId,
        BlackPlayerId = g.BlackPlayerId,
        Result = g.Result,
        Moves = g.Moves.Select(ToRecord).ToList()
    };

    public static MoveRecord ToRecord(this Move m) => new()
    {
        Id = Guid.NewGuid(), // on peut aussi générer côté DB, mais simple ici
        Ply = m.Ply,
        Notation = m.Notation,
        PlayedAtUtc = m.PlayedAtUtc
    };

    public static Move ToDomain(this MoveRecord r)
        => new Move(r.Ply, r.Notation, r.PlayedAtUtc);

    public static Game ToDomain(this GameRecord r)
    {
        var g = new Game(r.Id, r.CompetitionId, r.WhitePlayerId, r.BlackPlayerId);
        var moves = r.Moves.OrderBy(m => m.Ply).Select(ToDomain).ToList();
        g.LoadFromPersistence(moves, r.Result);
        return g;
    }
}
