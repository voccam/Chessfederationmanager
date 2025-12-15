using ChessFederationManager.Application.Services;
using ChessFederationManager.Tests.Fakes;
using Xunit;

namespace ChessFederationManager.Tests.Application;

public class PlayerServiceTests
{
    [Fact]
    public async Task CreateAsync_DuplicateEmail_ShouldThrow()
    {
        var repo = new InMemoryPlayerRepository();
        var service = new PlayerService(repo);

        await service.CreateAsync("A", "B", "dup@test.com");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync("C", "D", "dup@test.com"));
    }
}
