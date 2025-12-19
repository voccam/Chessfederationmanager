using Avalonia.Controls;
using Avalonia.Interactivity;
using ChessFederationManager.UI.ViewModels;

namespace ChessFederationManager.UI.Views;

public partial class CompetitionsView : UserControl
{
    private CompetitionsViewModel Vm => (CompetitionsViewModel)DataContext!;

    public CompetitionsView()
    {
        InitializeComponent();
    }

    private async void Refresh_Click(object? sender, RoutedEventArgs e)
        => await Vm.RefreshAsync();

    private async void Create_Click(object? sender, RoutedEventArgs e)
        => await Vm.CreateAsync();

    private async void Delete_Click(object? sender, RoutedEventArgs e)
        => await Vm.DeleteSelectedAsync();

    private async void Register_Click(object? sender, RoutedEventArgs e)
        => await Vm.RegisterSelectedAsync();

    private async void Unregister_Click(object? sender, RoutedEventArgs e)
        => await Vm.UnregisterSelectedAsync();
}
