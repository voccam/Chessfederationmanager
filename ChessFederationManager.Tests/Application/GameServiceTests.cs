using ChessFederationManager.Application.Services;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Domain.Enums;
using CompetitionMemoryRepo = ChessFederationManager.Infrastructure.Repositories.InMemory.InMemoryCompetitionRepository;
using GameMemoryRepo = ChessFederationManager.Infrastructure.Repositories.InMemory.InMemoryGameRepository;
using PlayerMemoryRepo = ChessFederationManager.Tests.Fakes.InMemoryPlayerRepository;

namespace ChessFederationManager.Tests.Application;

public class GameServiceTests
{
    [Fact]
    public async Task SetResultAsync_UpdatesElo_And_PreventsDoubleResult()
    {
        var playerRepo = new PlayerMemoryRepo();
        var competitionRepo = new CompetitionMemoryRepo();
        var gameRepo = new GameMemoryRepo();

        var white = new Player("Alice", "White", "alice@test.com", 1200);
        var black = new Player("Bob", "Black", "bob@test.com", 1200);

        await playerRepo.AddAsync(white);
        await playerRepo.AddAsync(black);

        var competition = new Competition("Cup", new DateOnly(2025, 5, 1), "Brussels");
        competition.Register(white);
        competition.Register(black);
        await competitionRepo.AddAsync(competition);

        var service = new GameService(gameRepo, competitionRepo, playerRepo);

        var game = await service.CreateGameAsync(competition.Id, white.Id, black.Id);
        await service.SetResultAsync(game.Id, GameResult.WhiteWin);

        var updatedWhite = await playerRepo.GetByIdAsync(white.Id);
        var updatedBlack = await playerRepo.GetByIdAsync(black.Id);

        Assert.Equal(1216, updatedWhite!.Elo);
        Assert.Equal(1184, updatedBlack!.Elo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SetResultAsync(game.Id, GameResult.Draw));
    }

    [Fact]
    public async Task CreateGameAsync_PlayersMustBeRegistered()
    {
        var playerRepo = new PlayerMemoryRepo();
        var competitionRepo = new CompetitionMemoryRepo();
        var gameRepo = new GameMemoryRepo();

        var white = new Player("Alice", "White", "alice2@test.com");
        var black = new Player("Bob", "Black", "bob2@test.com");

        await playerRepo.AddAsync(white);
        await playerRepo.AddAsync(black);

        var competition = new Competition("Cup", new DateOnly(2025, 5, 1), "Brussels");
        competition.Register(white);
        await competitionRepo.AddAsync(competition);

        var service = new GameService(gameRepo, competitionRepo, playerRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateGameAsync(competition.Id, white.Id, black.Id));
    }

    [Fact]
    public async Task AddMoveAsync_ShouldFail_WhenGameFinished()
    {
        var playerRepo = new PlayerMemoryRepo();
        var competitionRepo = new CompetitionMemoryRepo();
        var gameRepo = new GameMemoryRepo();

        var white = new Player("Alice", "White", "alice3@test.com");
        var black = new Player("Bob", "Black", "bob3@test.com");

        await playerRepo.AddAsync(white);
        await playerRepo.AddAsync(black);

        var competition = new Competition("Cup", new DateOnly(2025, 5, 1), "Brussels");
        competition.Register(white);
        competition.Register(black);
        await competitionRepo.AddAsync(competition);

        var service = new GameService(gameRepo, competitionRepo, playerRepo);

        var game = await service.CreateGameAsync(competition.Id, white.Id, black.Id);
        await service.SetResultAsync(game.Id, GameResult.Draw);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AddMoveAsync(game.Id, 1, "e4"));
    }
}
