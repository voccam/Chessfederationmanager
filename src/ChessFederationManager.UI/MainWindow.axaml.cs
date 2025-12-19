using Avalonia.Controls;
using ChessFederationManager.UI.Composition;
using ChessFederationManager.UI.ViewModels;

namespace ChessFederationManager.UI;

public partial class MainWindow : Window
{
    private readonly AppServices _services;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _services = new AppServices();
        var playersVm = new PlayersViewModel(_services.PlayerService);
        var competitionsVm = new CompetitionsViewModel(_services.CompetitionService, _services.PlayerService);
        var gamesVm = new GamesViewModel(_services.CompetitionService, _services.GameService, _services.PlayerService);
        var leaderboardVm = new LeaderboardViewModel(_services.PlayerService);

        _viewModel = new MainViewModel(playersVm, competitionsVm, gamesVm, leaderboardVm);
        DataContext = _viewModel;

        this.Opened += async (_, _) => await _viewModel.LoadAsync();
        this.Closed += (_, _) => _services.Dispose();
    }
}
