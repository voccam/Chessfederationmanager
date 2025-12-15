namespace ChessFederationManager.Domain.Entities;

/// <summary>
/// Represents a chess competition organized by the federation.
/// </summary>
public class Competition
{
    private readonly List<Registration> _registrations = new();
    private readonly List<Game> _games = new();

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Location { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();
    public IReadOnlyCollection<Game> Games => _games.AsReadOnly();

    public Competition(string name, DateOnly startDate, DateOnly endDate, string location)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("The end date must be after the start date.", nameof(endDate));
        }

        Id = Guid.NewGuid();
        Name = RequireText(name, nameof(name));
        Location = RequireText(location, nameof(location));
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Rename(string name) => Name = RequireText(name, nameof(name));

    public void ChangeLocation(string location) => Location = RequireText(location, nameof(location));

    public void Reschedule(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentException("The end date must be after the start date.", nameof(end));
        }

        StartDate = start;
        EndDate = end;
    }

    public Registration RegisterPlayer(Player player, decimal feeAmount, DateTime? registeredAt = null)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (_registrations.Any(r => r.Player.Id == player.Id))
        {
            throw new InvalidOperationException("Player already registered.");
        }

        var registration = new Registration(Id, player, feeAmount, registeredAt);
        _registrations.Add(registration);
        return registration;
    }

    public Game ScheduleGame(Player whitePlayer, Player blackPlayer, int round, DateTime scheduledAt)
    {
        EnsurePlayerRegistered(whitePlayer);
        EnsurePlayerRegistered(blackPlayer);

        var game = new Game(Id, whitePlayer, blackPlayer, round, scheduledAt);
        _games.Add(game);
        return game;
    }

    public IEnumerable<Game> GetGamesForRound(int round) => _games.Where(g => g.Round == round);

    public IEnumerable<Game> GetGamesForPlayer(Guid playerId) =>
        _games.Where(g => g.WhitePlayer.Id == playerId || g.BlackPlayer.Id == playerId);

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        return value.Trim();
    }

    private void EnsurePlayerRegistered(Player player)
    {
        if (!_registrations.Any(r => r.Player.Id == player.Id))
        {
            throw new InvalidOperationException($"Player {player} is not registered in {Name}.");
        }
    }
}
