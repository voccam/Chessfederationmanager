using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChessFederationManager.Application.Services;
using ChessFederationManager.Domain.Entities;

namespace ChessFederationManager.UI.ViewModels;

public sealed class CompetitionsViewModel : INotifyPropertyChanged
{
    private readonly CompetitionService _competitionService;
    private readonly PlayerService _playerService;

    private IReadOnlyList<Player> _allPlayers = Array.Empty<Player>();
    private Task _registrationsLoader = Task.CompletedTask;

    public ObservableCollection<Competition> Competitions { get; } = new();
    public ObservableCollection<Player> RegisteredPlayers { get; } = new();
    public ObservableCollection<Player> AvailablePlayers { get; } = new();

    private string _name = "";
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

    private DateTimeOffset? _startDate = DateTimeOffset.Now;
    public DateTimeOffset? StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

    private string _location = "";
    public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

    private string _status = "";
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    private Competition? _selectedCompetition;
    public Competition? SelectedCompetition
    {
        get => _selectedCompetition;
        set
        {
            if (_selectedCompetition == value) return;
            _selectedCompetition = value;
            OnPropertyChanged();
            _registrationsLoader = LoadSelectedCompetitionAsync();
        }
    }

    private Player? _selectedAvailablePlayer;
    public Player? SelectedAvailablePlayer { get => _selectedAvailablePlayer; set { _selectedAvailablePlayer = value; OnPropertyChanged(); } }

    private Player? _selectedRegisteredPlayer;
    public Player? SelectedRegisteredPlayer { get => _selectedRegisteredPlayer; set { _selectedRegisteredPlayer = value; OnPropertyChanged(); } }

    public CompetitionsViewModel(CompetitionService competitionService, PlayerService playerService)
    {
        _competitionService = competitionService;
        _playerService = playerService;
    }

    public async Task LoadAsync()
    {
        try
        {
            _allPlayers = await _playerService.GetAllAsync();
            var competitions = await _competitionService.GetAllAsync();

            var selectedId = SelectedCompetition?.Id;

            Competitions.Clear();
            foreach (var comp in competitions.OrderBy(c => c.StartDate).ThenBy(c => c.Name))
                Competitions.Add(comp);

            if (selectedId is Guid id)
                SelectedCompetition = Competitions.FirstOrDefault(c => c.Id == id);
            else
                SelectedCompetition = Competitions.FirstOrDefault();

            if (SelectedCompetition is null)
            {
                RegisteredPlayers.Clear();
                AvailablePlayers.Clear();
            }
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }

        await _registrationsLoader;
    }

    public async Task RefreshAsync() => await LoadAsync();

    public async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Location) || StartDate is null)
        {
            Status = "Name, date and location are required.";
            return;
        }

        try
        {
            var dateOnly = DateOnly.FromDateTime(StartDate.Value.Date);
            var created = await _competitionService.CreateAsync(Name, dateOnly, Location);

            Name = "";
            Location = "";
            StartDate = DateTimeOffset.Now;

            Status = "Competition created.";
            await LoadAsync();
            SelectedCompetition = Competitions.FirstOrDefault(c => c.Id == created.Id) ?? SelectedCompetition;
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task DeleteSelectedAsync()
    {
        if (SelectedCompetition is null)
        {
            Status = "Select a competition first.";
            return;
        }

        try
        {
            await _competitionService.DeleteAsync(SelectedCompetition.Id);
            Status = "Competition deleted.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task RegisterSelectedAsync()
    {
        if (SelectedCompetition is null || SelectedAvailablePlayer is null)
        {
            Status = "Select a competition and a player.";
            return;
        }

        try
        {
            await _competitionService.RegisterPlayerAsync(SelectedCompetition.Id, SelectedAvailablePlayer.Id);
            Status = $"Player registered: {SelectedAvailablePlayer.FirstName} {SelectedAvailablePlayer.LastName}.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task UnregisterSelectedAsync()
    {
        if (SelectedCompetition is null || SelectedRegisteredPlayer is null)
        {
            Status = "Select a registered player.";
            return;
        }

        try
        {
            await _competitionService.UnregisterPlayerAsync(SelectedCompetition.Id, SelectedRegisteredPlayer.Id);
            Status = $"Player removed: {SelectedRegisteredPlayer.FirstName} {SelectedRegisteredPlayer.LastName}.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private async Task LoadSelectedCompetitionAsync()
    {
        RegisteredPlayers.Clear();
        AvailablePlayers.Clear();
        SelectedAvailablePlayer = null;
        SelectedRegisteredPlayer = null;

        if (SelectedCompetition is null) return;

        try
        {
            var comp = await _competitionService.GetByIdAsync(SelectedCompetition.Id);
            if (comp is null)
            {
                Status = "Competition not found.";
                return;
            }

            var registeredIds = comp.Registrations.Select(r => r.PlayerId).ToHashSet();

            var registeredPlayers = _allPlayers
                .Where(p => registeredIds.Contains(p.Id))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            foreach (var player in registeredPlayers)
                RegisteredPlayers.Add(player);

            var availablePlayers = _allPlayers
                .Where(p => !registeredIds.Contains(p.Id))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName);

            foreach (var player in availablePlayers)
                AvailablePlayers.Add(player);

            SelectedAvailablePlayer = AvailablePlayers.FirstOrDefault();
            SelectedRegisteredPlayer = RegisteredPlayers.FirstOrDefault();
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
