namespace ChessFederationManager.Infrastructure;

/// <summary>
/// Provides helper methods to build connection strings for the EF Core context.
/// </summary>
public static class DatabaseConfig
{
    private const string DefaultFileName = "chess_federation.db";

    public static string DefaultConnectionString(string? databasePath = null)
    {
        var path = string.IsNullOrWhiteSpace(databasePath)
            ? Path.Combine(GetDefaultDirectory(), DefaultFileName)
            : databasePath;

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return $"Data Source={path}";
    }

    private static string GetDefaultDirectory()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseDir, "ChessFederationManager");
    }
}
