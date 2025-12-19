using ChessFederationManager.Application.Services;
using ChessFederationManager.Infrastructure;
using ChessFederationManager.Infrastructure.Data;
using ChessFederationManager.Infrastructure.Repositories.EfCore;
using System;
using System.IO;


namespace ChessFederationManager.UI.Composition;

public sealed class AppServices : IDisposable
{
    private readonly ChessDbContext _db;

    public PlayerService PlayerService { get; }
    public CompetitionService CompetitionService { get; }
    public GameService GameService { get; }

    public AppServices()
    {
        Directory.CreateDirectory("data");
        var cs = DatabaseConfig.DefaultConnectionString("data/chess.db");

        _db = DbContextFactory.Create(cs);

        var playerRepo = new EfPlayerRepository(_db);
        var competitionRepo = new EfCompetitionRepository(_db);
        var gameRepo = new EfGameRepository(_db);

        PlayerService = new PlayerService(playerRepo);
        CompetitionService = new CompetitionService(competitionRepo, playerRepo);
        GameService = new GameService(gameRepo, competitionRepo, playerRepo);
    }

    public void Dispose() => _db.Dispose();
}
