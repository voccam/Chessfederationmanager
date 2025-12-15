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

    public static Competition ToDomain(this CompetitionRecord r)
    {
        var c = new Competition(r.Id, r.Name, r.StartDate, r.Location);

        // On recharge les inscriptions (si ton Domain n’a pas de méthode, on le fait via Register)
        // MAIS Register requiert un Player -> pas dispo ici.
        // Donc on conseille d'ajouter une méthode d'hydratation côté Competition.
        // Si tu ne l’as pas, on ignore les registrations ici et on les gère via repository dédié plus tard.
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

    public static Game ToDomain(this GameRecord r)
    {
        var g = new Game(r.Id, r.CompetitionId, r.WhitePlayerId, r.BlackPlayerId);

        // Si tu as LoadFromPersistence :
        // g.LoadFromPersistence(r.Moves.OrderBy(m => m.Ply).Select(ToDomainMove), r.Result);

        // Sinon : on fait le minimum (result + moves via méthodes publiques)
        g.SetResult(r.Result);
        foreach (var mv in r.Moves.OrderBy(m => m.Ply))
            g.AddMove(new Move(mv.Ply, mv.Notation, mv.PlayedAtUtc));

        return g;
    }
}
