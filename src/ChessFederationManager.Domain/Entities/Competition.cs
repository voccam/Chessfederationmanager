namespace ChessFederationManager.Domain.Entities;

public sealed class Competition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateOnly StartDate { get; private set; }
    public string Location { get; private set; }

    private readonly List<Registration> _registrations = new();
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();

    public Competition(string name, DateOnly startDate, string location)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location is required.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        StartDate = startDate;
        Location = location.Trim();
    }

    public Competition(Guid id, string name, DateOnly startDate, string location)
        : this(name, startDate, location)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.");
        Id = id;
    }

    public void UpdateInfo(string name, DateOnly startDate, string location)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location is required.");

        Name = name.Trim();
        StartDate = startDate;
        Location = location.Trim();
    }

    public Registration Register(Player player)
    {
        if (player is null) throw new ArgumentNullException(nameof(player));

        // règle métier: pas de doublon d'inscription
        if (_registrations.Any(r => r.PlayerId == player.Id))
            throw new InvalidOperationException("Player is already registered in this competition.");

        var reg = new Registration(Id, player.Id, DateTimeOffset.UtcNow);
        _registrations.Add(reg);
        return reg;
    }

    public void Unregister(Guid playerId)
    {
        var reg = _registrations.FirstOrDefault(r => r.PlayerId == playerId);
        if (reg is null) return;
        _registrations.Remove(reg);
    }

    public bool IsPlayerRegistered(Guid playerId)
        => _registrations.Any(r => r.PlayerId == playerId);

    public void LoadRegistrations(IEnumerable<Registration> registrations)
    {
        _registrations.Clear();
        _registrations.AddRange(registrations);
    }
}
