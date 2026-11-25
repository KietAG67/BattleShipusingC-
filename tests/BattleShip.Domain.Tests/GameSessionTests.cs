using BattleShip.Application;
using BattleShip.Domain;
using Xunit;

namespace BattleShip.Domain.Tests;

public class GameSessionTests
{
    [Fact]
    public void CannotStartBeforeBothFleetsArePlaced()
    {
        var session = new GameSession();

        Assert.Throws<InvalidOperationException>(() => session.Start());
    }

    [Fact]
    public void AHitKeepsThePlayerTurn()
    {
        var session = CreateReadySession();

        var result = session.PlayerShoot(new Cell(0, 0));

        Assert.Equal(ShotResult.Hit, result);
        Assert.True(session.IsPlayerTurn);
        Assert.Equal(1, session.PlayerShotCount);
    }

    [Fact]
    public void AMissSwitchesTurnBetweenPlayerAndBot()
    {
        var session = CreateReadySession();

        var playerMiss = session.PlayerShoot(new Cell(9, 9));
        Assert.Equal(ShotResult.Miss, playerMiss);
        Assert.False(session.IsPlayerTurn);

        var botMiss = session.BotShoot(new Cell(9, 9));
        Assert.Equal(ShotResult.Miss, botMiss);
        Assert.True(session.IsPlayerTurn);
    }

    [Fact]
    public void TimeoutTurnSwitchesTurnBetweenPlayerAndBot()
    {
        var session = CreateReadySession();
        Assert.True(session.IsPlayerTurn);

        session.TimeoutTurn();
        Assert.False(session.IsPlayerTurn);

        session.TimeoutTurn();
        Assert.True(session.IsPlayerTurn);
    }

    [Fact]
    public void TimeoutTurnThrowsWhenGameIsNotInProgress()
    {
        var session = new GameSession();

        Assert.Throws<InvalidOperationException>(() => session.TimeoutTurn());
    }

    [Fact]
    public void TryTimeoutTurnAppliesAfterThirtySeconds()
    {
        var session = CreateReadySession();
        var turnStart = session.TurnStartedAtUtc!.Value;

        Assert.False(session.TryTimeoutTurn(turnStart.AddSeconds(29)));
        Assert.True(session.IsPlayerTurn);

        Assert.True(session.TryTimeoutTurn(turnStart.AddSeconds(GameSession.TurnTimeLimitSeconds)));
        Assert.False(session.IsPlayerTurn);
    }

    [Fact]
    public void PlayerWinsWhenAllBotShipsAreSunk()
    {
        var session = CreateReadySession();

        for (var row = 0; row < Fleet.Default.Count; row++)
        {
            var length = Fleet.Default[row].Length;
            for (var col = 0; col < length; col++)
            {
                session.PlayerShoot(new Cell(row, col));
            }
        }

        Assert.Equal(GameStatus.PlayerWon, session.Status);
        Assert.NotNull(session.FinishedAtUtc);
    }

    [Fact]
    public void BotWinsWhenAllPlayerShipsAreSunk()
    {
        var session = CreateReadySession();
        session.PlayerShoot(new Cell(9, 9)); // Người chơi bắn trượt -> chuyển lượt cho bot

        for (var row = 0; row < Fleet.Default.Count; row++)
        {
            var length = Fleet.Default[row].Length;
            for (var col = 0; col < length; col++)
            {
                session.BotShoot(new Cell(row, col));
            }
        }

        Assert.Equal(GameStatus.BotWon, session.Status);
        Assert.NotNull(session.FinishedAtUtc);
    }

    [Fact]
    public void ResetClearsBoardsAndRestoresSetupStatus()
    {
        var session = CreateReadySession();
        session.PlayerShoot(new Cell(0, 0));

        session.Reset();

        Assert.Equal(GameStatus.Setup, session.Status);
        Assert.True(session.IsPlayerTurn);
        Assert.Empty(session.PlayerBoard.Ships);
        Assert.Empty(session.BotBoard.Ships);
        Assert.Equal(0, session.TotalShots);
        Assert.Empty(session.ShotHistory);
    }

    private static GameSession CreateReadySession()
    {
        var session = new GameSession();
        PlaceDefaultFleet(session.PlayerBoard);
        PlaceDefaultFleet(session.BotBoard);
        session.Start();
        return session;
    }

    private static void PlaceDefaultFleet(Board board)
    {
        var row = 0;
        foreach (var (name, length) in Fleet.Default)
        {
            board.PlaceShip(name, length, new Cell(row, 0), Orientation.Horizontal);
            row++;
        }
    }
}
