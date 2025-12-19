using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChessFederationManager.Application.Services;

namespace ChessFederationManager.UI.ViewModels;

public sealed class LeaderboardViewModel : INotifyPropertyChanged
{
    private readonly PlayerService _playerService;

    public ObservableCollection<RankedPlayer> Players { get; } = new();

    private string _status = "";
    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }

    public LeaderboardViewModel(PlayerService playerService)
        => _playerService = playerService;

    public async Task LoadAsync()
    {
        try
        {
            var players = await _playerService.GetAllAsync();
            Players.Clear();

            var ordered = players
                .OrderByDescending(p => p.Elo)
                .ThenBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            var rank = 1;
            foreach (var player in ordered)
                Players.Add(new RankedPlayer(rank++, player.FirstName, player.LastName, player.Email, player.Elo));

            Status = $"Dernière mise à jour: {DateTime.Now:t}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public Task RefreshAsync() => LoadAsync();

    public sealed record RankedPlayer(int Rank, string FirstName, string LastName, string Email, int Elo)
    {
        public string FullName => $"{LastName} {FirstName}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
