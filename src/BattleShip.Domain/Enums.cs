namespace BattleShip.Domain;

public enum Orientation
{
    Horizontal,
    Vertical,
}

public enum CellState
{
    Unknown,
    Ship,
    Hit,
    Miss,
    Sunk,
}

public enum ShotResult
{
    Hit,
    Miss,
    Sunk,
    AlreadyShot,
    GameOver,
}

public enum GameStatus
{
    Setup,
    InProgress,
    PlayerWon,
    BotWon,
}
