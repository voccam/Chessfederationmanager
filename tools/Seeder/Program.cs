using ChessFederationManager.Application.Services;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Domain.Enums;
using ChessFederationManager.Infrastructure;
using ChessFederationManager.Infrastructure.Data;
using ChessFederationManager.Infrastructure.Repositories.EfCore;

var dbPath = args.Length > 0 ? args[0] : Path.Combine("data", "chess.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? ".");

var connectionString = DatabaseConfig.DefaultConnectionString(dbPath);
Console.WriteLine($"Seeding database at: {dbPath}");

using var db = DbContextFactory.Create(connectionString);

var playerRepo = new EfPlayerRepository(db);
var competitionRepo = new EfCompetitionRepository(db);
var gameRepo = new EfGameRepository(db);

var playerService = new PlayerService(playerRepo);
var competitionService = new CompetitionService(competitionRepo, playerRepo);
var gameService = new GameService(gameRepo, competitionRepo, playerRepo);

// ---- Players ----
var players = new[]
{
    new PlayerSeed("Mathis", "Vandesmal", "mathisvds932@gmail.com", 1171),
    new PlayerSeed("Amedeo", "Mastrogiovanni", "amedeo.mastrogiovanni@gmail.com", 1231),
    new PlayerSeed("Melvyn", "Bormans", "melvyn.bormans@gmail.com", 1215),
    new PlayerSeed("Mathieu", "Franchimont", "mathieu.franchimont@gmail.com", 1183),
    new PlayerSeed("Alice", "White", "alice@test.com", 1500),
    new PlayerSeed("Bob", "Black", "bob@test.com", 1450)
};

var playerByEmail = new Dictionary<string, Player>(StringComparer.OrdinalIgnoreCase);
foreach (var seed in players)
{
    var existing = await playerRepo.GetByEmailAsync(seed.Email);
    if (existing is null)
    {
        existing = await playerService.CreateAsync(seed.FirstName, seed.LastName, seed.Email, seed.Elo);
        Console.WriteLine($"Created player {existing.FirstName} {existing.LastName} ({existing.Elo})");
    }
    playerByEmail[seed.Email] = existing;
}

// ---- Competition ----
const string compName = "Open 2025";
var competitions = await competitionService.GetAllAsync();
var comp = competitions.FirstOrDefault(c => c.Name.Equals(compName, StringComparison.OrdinalIgnoreCase));
if (comp is null)
{
    comp = await competitionService.CreateAsync(compName, new DateOnly(2025, 3, 1), "Bruxelles");
    Console.WriteLine($"Created competition {comp.Name}");
}

// Refresh to get current registrations from DB
comp = await competitionService.GetByIdAsync(comp.Id) ?? comp;

// Register all players to the competition (ignore duplicates)
foreach (var player in playerByEmail.Values)
{
    try
    {
        if (!comp.IsPlayerRegistered(player.Id))
        {
            await competitionService.RegisterPlayerAsync(comp.Id, player.Id);
            Console.WriteLine($"Registered {player.FirstName} to {comp.Name}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Skip register {player.Email}: {ex.Message}");
    }
}

// Reload to ensure registrations are present
comp = await competitionService.GetByIdAsync(comp.Id) ?? comp;

// ---- Games ----
var plannedGames = new[]
{
    new GameSeed("alice@test.com", "bob@test.com", GameResult.WhiteWin, new[]{"e4", "e5", "Nf3"}),
    new GameSeed("mathisvds932@gmail.com", "melvyn.bormans@gmail.com", GameResult.BlackWin, new[]{"d4", "Nf6", "c4"}),
    new GameSeed("amedeo.mastrogiovanni@gmail.com", "mathieu.franchimont@gmail.com", GameResult.Draw, new[]{"c4", "e6", "Nc3"})
};

var existingGames = await gameService.GetByCompetitionIdAsync(comp.Id);

foreach (var gameSeed in plannedGames)
{
    if (!playerByEmail.TryGetValue(gameSeed.WhiteEmail, out var white) ||
        !playerByEmail.TryGetValue(gameSeed.BlackEmail, out var black))
    {
        Console.WriteLine($"Skip game: missing player for {gameSeed.WhiteEmail} vs {gameSeed.BlackEmail}");
        continue;
    }

    var already = existingGames.FirstOrDefault(g =>
        g.WhitePlayerId == white.Id && g.BlackPlayerId == black.Id);

    Game game;
    if (already is null)
    {
        try
        {
            game = await gameService.CreateGameAsync(comp.Id, white.Id, black.Id);
            Console.WriteLine($"Created game {white.LastName} vs {black.LastName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Skip game {white.Email} vs {black.Email}: {ex.Message}");
            continue;
        }
    }
    else
    {
        game = already;
    }

    if (game.Result == GameResult.NotPlayed)
    {
        var ply = 1;
        foreach (var move in gameSeed.Moves)
        {
            await gameService.AddMoveAsync(game.Id, ply++, move);
        }
        await gameService.SetResultAsync(game.Id, gameSeed.Result);
        Console.WriteLine($"Recorded game {white.LastName} vs {black.LastName}: {gameSeed.Result}");
    }
    else
    {
        Console.WriteLine($"Game already finished: {white.LastName} vs {black.LastName}");
    }
}

// ---- Leaderboard preview ----
var allPlayers = await playerService.GetAllAsync();
Console.WriteLine("\nLeaderboard (Top Elo):");
foreach (var p in allPlayers.OrderByDescending(p => p.Elo).Take(10))
    Console.WriteLine($"{p.LastName,-15} {p.FirstName,-10} Elo {p.Elo}");

Console.WriteLine("\nSeeding complete.");

record PlayerSeed(string FirstName, string LastName, string Email, int Elo);
record GameSeed(string WhiteEmail, string BlackEmail, GameResult Result, string[] Moves);
