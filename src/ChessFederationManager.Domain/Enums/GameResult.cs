namespace ChessFederationManager.Domain.Enums;

/// <summary>
/// Represents the outcome of a chess game.
/// </summary>
public enum GameResult
{
    Undecided = 0,
    WhiteWin = 1,
    BlackWin = 2,
    Draw = 3,
    WhiteForfeit = 4,
    BlackForfeit = 5
}
