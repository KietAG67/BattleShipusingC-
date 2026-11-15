using BattleShip.Application.Interfaces;
using BattleShip.Domain;

namespace BattleShip.Application;

public sealed class GameSession
{
    public const int TurnTimeLimitSeconds = 30;
    public const string PlayerActor = "Player";
    public const string BotActor = "Bot";

    private readonly List<ShotHistoryEntry> _shotHistory = [];

    public Board PlayerBoard { get; private set; } = new();

    public Board BotBoard { get; private set; } = new();

    public GameStatus Status { get; private set; } = GameStatus.Setup;

    public bool IsPlayerTurn { get; private set; } = true;

    public int PlayerShotCount { get; private set; }

    public int BotShotCount { get; private set; }

    public int TotalShots => PlayerShotCount + BotShotCount;

    public DateTime? StartedAtUtc { get; private set; }

    public DateTime? FinishedAtUtc { get; private set; }

    public DateTime? TurnStartedAtUtc { get; private set; }

    public IReadOnlyList<ShotHistoryEntry> ShotHistory => _shotHistory;

    public void Start()
    {
        if (PlayerBoard.Ships.Count != Fleet.Default.Count || BotBoard.Ships.Count != Fleet.Default.Count)
            throw new InvalidOperationException("Must deploy your entire fleet before starting..");

        var nowUtc = DateTime.UtcNow;
        Status = GameStatus.InProgress;
        IsPlayerTurn = true;
        PlayerShotCount = 0;
        BotShotCount = 0;
        StartedAtUtc = nowUtc;
        TurnStartedAtUtc = nowUtc;
        FinishedAtUtc = null;
        _shotHistory.Clear();
    }

    public ShotResult PlayerShoot(Cell cell)
    {
        EnsurePlayerTurn();
        var result = BotBoard.Shoot(cell);
        RecordAndApplyResult(cell, result, playerShot: true);
        return result;
    }

    public ShotResult BotShoot(Cell cell)
    {
        EnsureBotTurn();
        var result = PlayerBoard.Shoot(cell);
        RecordAndApplyResult(cell, result, playerShot: false);
        return result;
    }

    public void TimeoutTurn()
    {
        if (Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game has not started or has already finished.");

        IsPlayerTurn = !IsPlayerTurn;
        TurnStartedAtUtc = DateTime.UtcNow;
    }

    public bool IsTurnExpired(DateTime? currentUtc = null)
    {
        if (Status != GameStatus.InProgress || TurnStartedAtUtc is null)
            return false;

        var now = currentUtc ?? DateTime.UtcNow;
        return (now - TurnStartedAtUtc.Value).TotalSeconds >= TurnTimeLimitSeconds;
    }

    public bool TryTimeoutTurn(DateTime? currentUtc = null)
    {
        if (!IsTurnExpired(currentUtc))
            return false;

        TimeoutTurn();
        return true;
    }

    public void Reset()
    {
        PlayerBoard = new Board();
        BotBoard = new Board();
        Status = GameStatus.Setup;
        IsPlayerTurn = true;
        PlayerShotCount = 0;
        BotShotCount = 0;
        StartedAtUtc = null;
        FinishedAtUtc = null;
        TurnStartedAtUtc = null;
        _shotHistory.Clear();
    }

    private void EnsurePlayerTurn()
    {
        if (Status != GameStatus.InProgress || !IsPlayerTurn)
            throw new InvalidOperationException("Not your turn.");
    }

    private void EnsureBotTurn()
    {
        if (Status != GameStatus.InProgress || IsPlayerTurn)
            throw new InvalidOperationException("Not bot's turn.");
    }

    private void RecordAndApplyResult(Cell target, ShotResult result, bool playerShot)
    {
        if (result == ShotResult.AlreadyShot)
            return;

        var nowUtc = DateTime.UtcNow;
        if (playerShot)
            PlayerShotCount++;
        else
            BotShotCount++;

        var actor = playerShot ? PlayerActor : BotActor;
        _shotHistory.Add(new ShotHistoryEntry(actor, target, result, nowUtc));

        if (result == ShotResult.GameOver)
        {
            Status = playerShot ? GameStatus.PlayerWon : GameStatus.BotWon;
            FinishedAtUtc = nowUtc;
            return;
        }

        if (result == ShotResult.Miss)
        {
            IsPlayerTurn = !playerShot;
        }

        TurnStartedAtUtc = nowUtc;
    }
}
