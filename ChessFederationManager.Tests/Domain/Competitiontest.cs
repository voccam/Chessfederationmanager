using ChessFederationManager.Domain.Entities;
using Xunit;

namespace ChessFederationManager.Tests.Domain;

public class CompetitionTests
{
    [Fact]
    public void Register_SamePlayerTwice_ShouldThrow()
    {
        var p = new Player("Mathis", "Vandesmal", "mathis@test.com");
        var c = new Competition("Open", new DateOnly(2025, 12, 1), "Brussels");

        c.Register(p);

        Assert.Throws<InvalidOperationException>(() => c.Register(p));
    }

    [Fact]
    public void LoadRegistrations_ShouldPreventDuplicateRegister()
    {
        var player = new Player("Ada", "Lovelace", "ada@test.com");
        var competition = new Competition("Masters", new DateOnly(2026, 6, 1), "Liege");
        var reg = new Registration(competition.Id, player.Id, DateTimeOffset.UtcNow);

        competition.LoadRegistrations(new[] { reg });

        Assert.True(competition.IsPlayerRegistered(player.Id));
        Assert.Throws<InvalidOperationException>(() => competition.Register(player));
    }
}
