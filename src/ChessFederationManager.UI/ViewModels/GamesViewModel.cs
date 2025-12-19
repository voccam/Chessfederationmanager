using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChessFederationManager.Application.Services;
using ChessFederationManager.Domain.Entities;
using ChessFederationManager.Domain.Enums;

namespace ChessFederationManager.UI.ViewModels;

public sealed class GamesViewModel : INotifyPropertyChanged
{
    private readonly CompetitionService _competitionService;
    private readonly GameService _gameService;
    private readonly PlayerService _playerService;

    private IReadOnlyList<Player> _allPlayers = Array.Empty<Player>();
    private IReadOnlyDictionary<Guid, Player> _playersById = new Dictionary<Guid, Player>();
    private Task _competitionLoader = Task.CompletedTask;

    public ObservableCollection<Competition> Competitions { get; } = new();
    public ObservableCollection<Player> CompetitionPlayers { get; } = new();
    public ObservableCollection<GameRow> Games { get; } = new();

    public IReadOnlyList<GameResult> GameResults { get; } =
        Enum.GetValues<GameResult>().Where(r => r != GameResult.NotPlayed).ToArray();

    private Competition? _selectedCompetition;
    public Competition? SelectedCompetition
    {
        get => _selectedCompetition;
        set
        {
            if (_selectedCompetition == value) return;
            _selectedCompetition = value;
            OnPropertyChanged();
            _competitionLoader = LoadCompetitionDetailsAsync();
        }
    }

    private GameRow? _selectedGame;
    public GameRow? SelectedGame { get => _selectedGame; set { _selectedGame = value; OnPropertyChanged(); } }

    private Player? _selectedWhitePlayer;
    public Player? SelectedWhitePlayer { get => _selectedWhitePlayer; set { _selectedWhitePlayer = value; OnPropertyChanged(); } }

    private Player? _selectedBlackPlayer;
    public Player? SelectedBlackPlayer { get => _selectedBlackPlayer; set { _selectedBlackPlayer = value; OnPropertyChanged(); } }

    private string _newMoveNotation = "";
    public string NewMoveNotation { get => _newMoveNotation; set { _newMoveNotation = value; OnPropertyChanged(); } }

    private GameResult _selectedResult = GameResult.WhiteWin;
    public GameResult SelectedResult { get => _selectedResult; set { _selectedResult = value; OnPropertyChanged(); } }

    private string _status = "";
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    public GamesViewModel(CompetitionService competitionService, GameService gameService, PlayerService playerService)
    {
        _competitionService = competitionService;
        _gameService = gameService;
        _playerService = playerService;
    }

    public async Task LoadAsync()
    {
        try
        {
            var players = await _playerService.GetAllAsync();
            _allPlayers = players;
            _playersById = players.ToDictionary(p => p.Id);

            var competitions = await _competitionService.GetAllAsync();
            var selectedId = SelectedCompetition?.Id;

            Competitions.Clear();
            foreach (var comp in competitions.OrderBy(c => c.StartDate).ThenBy(c => c.Name))
                Competitions.Add(comp);

            if (selectedId is Guid id)
                SelectedCompetition = Competitions.FirstOrDefault(c => c.Id == id) ?? Competitions.FirstOrDefault();
            else
                SelectedCompetition = Competitions.FirstOrDefault();

            if (SelectedCompetition is null)
            {
                CompetitionPlayers.Clear();
                Games.Clear();
                SelectedGame = null;
            }
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }

        await _competitionLoader;
    }

    public async Task RefreshAsync() => await LoadAsync();

    public async Task CreateGameAsync()
    {
        if (SelectedCompetition is null || SelectedWhitePlayer is null || SelectedBlackPlayer is null)
        {
            Status = "Select a competition and two players.";
            return;
        }

        if (SelectedWhitePlayer.Id == SelectedBlackPlayer.Id)
        {
            Status = "Players must be different.";
            return;
        }

        try
        {
            var game = await _gameService.CreateGameAsync(
                SelectedCompetition.Id,
                SelectedWhitePlayer.Id,
                SelectedBlackPlayer.Id);

            Status = "Game created.";
            await ReloadGamesAndSelectAsync(game.Id);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task AddMoveAsync()
    {
        if (SelectedGame is null)
        {
            Status = "Select a game first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewMoveNotation))
        {
            Status = "Enter a move notation.";
            return;
        }

        try
        {
            var nextPly = SelectedGame.Game.Moves.Count + 1;
            await _gameService.AddMoveAsync(SelectedGame.Game.Id, nextPly, NewMoveNotation);

            var gameId = SelectedGame.Game.Id;
            NewMoveNotation = "";
            Status = "Move added.";
            await ReloadGamesAndSelectAsync(gameId);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task RecordResultAsync()
    {
        if (SelectedGame is null)
        {
            Status = "Select a game.";
            return;
        }

        try
        {
            await _gameService.SetResultAsync(SelectedGame.Game.Id, SelectedResult);
            Status = "Result saved.";
            await ReloadGamesAndSelectAsync(SelectedGame.Game.Id);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public async Task DeleteSelectedGameAsync()
    {
        if (SelectedGame is null)
        {
            Status = "Select a game.";
            return;
        }

        try
        {
            var deletedId = SelectedGame.Game.Id;
            await _gameService.DeleteAsync(deletedId);
            Status = "Game deleted.";
            await ReloadGamesAndSelectAsync(null);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private async Task ReloadGamesAndSelectAsync(Guid? targetGameId)
    {
        await LoadCompetitionDetailsAsync();
        if (targetGameId is Guid id)
            SelectedGame = Games.FirstOrDefault(g => g.Game.Id == id);
        else
            SelectedGame = Games.FirstOrDefault();
    }

    private async Task LoadCompetitionDetailsAsync()
    {
        CompetitionPlayers.Clear();
        Games.Clear();
        SelectedGame = null;

        if (SelectedCompetition is null)
        {
            SelectedWhitePlayer = null;
            SelectedBlackPlayer = null;
            return;
        }

        try
        {
            var comp = await _competitionService.GetByIdAsync(SelectedCompetition.Id);
            if (comp is null)
            {
                Status = "Competition not found.";
                return;
            }

            var registeredIds = comp.Registrations.Select(r => r.PlayerId).ToHashSet();

            var registered = _allPlayers
                .Where(p => registeredIds.Contains(p.Id))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();

            foreach (var player in registered)
                CompetitionPlayers.Add(player);

            EnsurePlayerSelections();

            var games = await _gameService.GetByCompetitionIdAsync(SelectedCompetition.Id);
            foreach (var game in games.OrderBy(g => g.Result == GameResult.NotPlayed ? 0 : 1)
                                      .ThenBy(g => g.Id))
            {
                Games.Add(new GameRow(
                    game,
                    FormatPlayer(game.WhitePlayerId),
                    FormatPlayer(game.BlackPlayerId)));
            }

            SelectedGame = Games.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private void EnsurePlayerSelections()
    {
        if (CompetitionPlayers.Count == 0)
        {
            SelectedWhitePlayer = null;
            SelectedBlackPlayer = null;
            return;
        }

        var desiredWhiteId = SelectedWhitePlayer?.Id;
        var desiredBlackId = SelectedBlackPlayer?.Id;

        SelectedWhitePlayer = desiredWhiteId is Guid whiteId
            ? CompetitionPlayers.FirstOrDefault(p => p.Id == whiteId) ?? CompetitionPlayers.FirstOrDefault()
            : CompetitionPlayers.FirstOrDefault();

        SelectedBlackPlayer = desiredBlackId is Guid blackId
            ? CompetitionPlayers.FirstOrDefault(p => p.Id == blackId && p.Id != SelectedWhitePlayer?.Id)
              ?? CompetitionPlayers.FirstOrDefault(p => p.Id != SelectedWhitePlayer?.Id)
              ?? SelectedWhitePlayer
            : CompetitionPlayers.FirstOrDefault(p => p.Id != SelectedWhitePlayer?.Id)
              ?? SelectedWhitePlayer;

        if (SelectedBlackPlayer?.Id == SelectedWhitePlayer?.Id && CompetitionPlayers.Count > 1)
            SelectedBlackPlayer = CompetitionPlayers.FirstOrDefault(p => p.Id != SelectedWhitePlayer?.Id);
    }

    private string FormatPlayer(Guid playerId)
        => _playersById.TryGetValue(playerId, out var player)
            ? $"{player.LastName} {player.FirstName} ({player.Elo})"
            : playerId.ToString();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public sealed record GameRow(Game Game, string WhitePlayer, string BlackPlayer)
    {
        public string ResultText => Game.Result switch
        {
            GameResult.WhiteWin => "1-0",
            GameResult.BlackWin => "0-1",
            GameResult.Draw => "½-½",
            _ => "In progress"
        };

        public string MovesDisplay => Game.Moves.Count == 0
            ? "-"
            : string.Join(", ", Game.Moves.Select(m => $"{m.Ply}. {m.Notation}"));
    }
}
