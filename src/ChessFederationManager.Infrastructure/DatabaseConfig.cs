namespace ChessFederationManager.Infrastructure;

public static class DatabaseConfig
{
    public static string DefaultConnectionString(string dbPath)
        => $"Data Source={dbPath}";
}
