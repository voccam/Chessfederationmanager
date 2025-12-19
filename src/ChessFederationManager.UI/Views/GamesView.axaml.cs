using Avalonia.Controls;
using Avalonia.Interactivity;
using ChessFederationManager.UI.ViewModels;

namespace ChessFederationManager.UI.Views;

public partial class GamesView : UserControl
{
    private GamesViewModel Vm => (GamesViewModel)DataContext!;

    public GamesView()
    {
        InitializeComponent();
    }

    private async void Refresh_Click(object? sender, RoutedEventArgs e)
        => await Vm.RefreshAsync();

    private async void CreateGame_Click(object? sender, RoutedEventArgs e)
        => await Vm.CreateGameAsync();

    private async void AddMove_Click(object? sender, RoutedEventArgs e)
        => await Vm.AddMoveAsync();

    private async void Result_Click(object? sender, RoutedEventArgs e)
        => await Vm.RecordResultAsync();

    private async void Delete_Click(object? sender, RoutedEventArgs e)
        => await Vm.DeleteSelectedGameAsync();
}
