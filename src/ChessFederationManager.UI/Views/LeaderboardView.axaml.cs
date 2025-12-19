using Avalonia.Controls;
using Avalonia.Interactivity;
using ChessFederationManager.UI.ViewModels;

namespace ChessFederationManager.UI.Views;

public partial class LeaderboardView : UserControl
{
    private LeaderboardViewModel Vm => (LeaderboardViewModel)DataContext!;

    public LeaderboardView()
    {
        InitializeComponent();
    }

    private async void Refresh_Click(object? sender, RoutedEventArgs e)
        => await Vm.RefreshAsync();
}
