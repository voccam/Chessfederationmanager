using ChessFederationManager.Application.Services;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Infrastructure;
using ChessFederationManager.Infrastructure.Data;
using ChessFederationManager.Infrastructure.Repositories.EfCore;
using Xunit;

namespace ChessFederationManager.Tests.Infrastructure;

public class EfCorePersistenceTests
{
    [Fact]
    public async Task Can_Save_And_Load_Player_With_Sqlite()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"chess_test_{Guid.NewGuid():N}.db");
        var cs = DatabaseConfig.DefaultConnectionString(dbPath);

        using var db = DbContextFactory.Create(cs);

        var playerRepo = new EfPlayerRepository(db);
        var service = new PlayerService(playerRepo);

        var created = await service.CreateAsync("Ada", "Lovelace", "ada@test.com", 1500);

        var loaded = await playerRepo.GetByIdAsync(created.Id);

        Assert.NotNull(loaded);
        Assert.Equal("ada@test.com", loaded!.Email);
        Assert.Equal(1500, loaded.Elo);

        // cleanup
        db.Dispose();
        if (File.Exists(dbPath)) File.Delete(dbPath);
    }

    [Fact]
    public async Task Competition_With_Registrations_Is_Persisted()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"chess_comp_test_{Guid.NewGuid():N}.db");
        var cs = DatabaseConfig.DefaultConnectionString(dbPath);

        using var db = DbContextFactory.Create(cs);
        var repo = new EfCompetitionRepository(db);

        var competition = new Competition("Spring Open", new DateOnly(2025, 4, 1), "Namur");
        var registeredPlayerId = Guid.NewGuid();
        competition.LoadRegistrations(new[]
        {
            new Registration(competition.Id, registeredPlayerId, DateTimeOffset.UtcNow)
        });

        await repo.AddAsync(competition);

        var loaded = await repo.GetByIdAsync(competition.Id);

        Assert.NotNull(loaded);
        Assert.True(loaded!.IsPlayerRegistered(registeredPlayerId));

        db.Dispose();
        if (File.Exists(dbPath)) File.Delete(dbPath);
    }
}
