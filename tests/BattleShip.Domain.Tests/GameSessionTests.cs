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
