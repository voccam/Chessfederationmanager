using ChessFederationManager.Application.Services;
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
}
