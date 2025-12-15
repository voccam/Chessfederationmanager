using ChessFederationManager.Domain.Entities;
using Xunit;

namespace ChessFederationManager.Tests.Domain;

public class GameTests
{
    [Fact]
    public void AddMove_PlyMustIncrease_ShouldThrow()
    {
        var g = new Game(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        g.AddMove(new Move(1, "e4"));
        Assert.Throws<InvalidOperationException>(() => g.AddMove(new Move(1, "e5")));
    }
}
