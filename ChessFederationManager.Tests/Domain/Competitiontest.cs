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
}
