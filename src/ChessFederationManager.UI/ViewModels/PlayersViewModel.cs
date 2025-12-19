using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ChessFederationManager.Application.Services;
using ChessFederationManager.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace ChessFederationManager.UI.ViewModels;

public sealed class PlayersViewModel : INotifyPropertyChanged
{
    private readonly PlayerService _playerService;

    public ObservableCollection<Player> Players { get; } = new();

    private string _firstName = "";
    public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); } }

    private string _lastName = "";
    public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); } }

    private string _email = "";
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

    private int _elo = 1200;
    public int Elo { get => _elo; set { _elo = value; OnPropertyChanged(); } }

    private string _status = "";
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    private Player? _selectedPlayer;
    public Player? SelectedPlayer { get => _selectedPlayer; set { _selectedPlayer = value; OnPropertyChanged(); } }

    public PlayersViewModel(PlayerService playerService)
    {
        _playerService = playerService;
    }

    public async Task LoadAsync()
    {
        Players.Clear();
        var players = await _playerService.GetAllAsync();
        foreach (var p in players.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
            Players.Add(p);

        Status = $"Loaded {Players.Count} player(s).";
    }

    public async Task AddAsync()
    {
        try
        {
            var created = await _playerService.CreateAsync(FirstName, LastName, Email, Elo);
            Players.Add(created);

            FirstName = ""; LastName = ""; Email = ""; Elo = 1200;
            Status = $"Player created: {created.FirstName} {created.LastName}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task DeleteSelectedAsync()
    {
        if (SelectedPlayer is null)
        {
            Status = "Select a player first.";
            return;
        }

        try
        {
            await _playerService.DeleteAsync(SelectedPlayer.Id);
            Players.Remove(SelectedPlayer);
            SelectedPlayer = null;
            Status = "Player deleted.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
