using Avalonia.Controls;
using Avalonia.Interactivity;
using ChessFederationManager.UI.ViewModels;

namespace ChessFederationManager.UI.Views;

public partial class PlayersView : UserControl
{
    private ChessFederationManager.UI.ViewModels.PlayersViewModel Vm => (ChessFederationManager.UI.ViewModels.PlayersViewModel)DataContext!;


    public PlayersView()
    {
        InitializeComponent();
    }

    private async void Refresh_Click(object? sender, RoutedEventArgs e)
        => await Vm.LoadAsync();

    private async void Add_Click(object? sender, RoutedEventArgs e)
        => await Vm.AddAsync();

    private async void Delete_Click(object? sender, RoutedEventArgs e)
        => await Vm.DeleteSelectedAsync();
}
