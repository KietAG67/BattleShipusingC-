using BattleShip.Domain;

namespace BattleShip.Application;

public sealed class GameSession
{
    public Board PlayerBoard { get; } = new();
    public Board BotBoard { get; } = new();
    public GameStatus Status { get; private set; } = GameStatus.Setup;
    public bool IsPlayerTurn { get; private set; } = true;

    public void Start()
    {
        if (PlayerBoard.Ships.Count != Fleet.Default.Count || BotBoard.Ships.Count != Fleet.Default.Count)
            throw new InvalidOperationException("Cần đặt đủ hạm đội trước khi bắt đầu.");

        Status = GameStatus.InProgress;
        IsPlayerTurn = true;
    }

    public ShotResult PlayerShoot(Cell cell)
    {
        EnsurePlayerTurn();
        var result = BotBoard.Shoot(cell);
        ApplyResult(result, playerShot: true);
        return result;
    }

    public ShotResult BotShoot(Cell cell)
    {
        if (Status != GameStatus.InProgress || IsPlayerTurn)
            throw new InvalidOperationException("Chưa đến lượt bot.");

        var result = PlayerBoard.Shoot(cell);
        ApplyResult(result, playerShot: false);
        return result;
    }

    public void Reset()
    {
        Status = GameStatus.Setup;
        IsPlayerTurn = true;
    }

    private void EnsurePlayerTurn()
    {
        if (Status != GameStatus.InProgress || !IsPlayerTurn)
            throw new InvalidOperationException("Không phải lượt người chơi.");
    }

    private void ApplyResult(ShotResult result, bool playerShot)
    {
        if (result == ShotResult.GameOver)
        {
            Status = playerShot ? GameStatus.PlayerWon : GameStatus.BotWon;
            return;
        }

        if (result == ShotResult.Miss)
            IsPlayerTurn = !playerShot;
    }
}
