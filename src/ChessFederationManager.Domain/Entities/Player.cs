namespace ChessFederationManager.Domain.Entities;

public sealed class Player
{
    public Guid Id { get; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public int Elo { get; private set; }

    public Player(string firstName, string lastName, string email, int elo = 1200)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("LastName is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");
        if (elo < 0) throw new ArgumentException("Elo must be >= 0.");

        Id = Guid.NewGuid();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        Elo = elo;
    }

    public void UpdateIdentity(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName is required.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("LastName is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
    }

    public void SetElo(int newElo)
    {
        if (newElo < 0) throw new ArgumentException("Elo must be >= 0.");
        Elo = newElo;
    }

    public override string ToString() => $"{LastName} {FirstName} ({Elo})";
}
