using System.Threading.Tasks;

namespace ChessFederationManager.UI.ViewModels;

public sealed class MainViewModel
{
    public PlayersViewModel Players { get; }
    public CompetitionsViewModel Competitions { get; }
    public GamesViewModel Games { get; }
    public LeaderboardViewModel Leaderboard { get; }

    public MainViewModel(
        PlayersViewModel players,
        CompetitionsViewModel competitions,
        GamesViewModel games,
        LeaderboardViewModel leaderboard)
    {
        Players = players;
        Competitions = competitions;
        Games = games;
        Leaderboard = leaderboard;
    }

    public async Task LoadAsync()
    {
        await Players.LoadAsync();
        await Competitions.LoadAsync();
        await Games.LoadAsync();
        await Leaderboard.LoadAsync();
    }
}
