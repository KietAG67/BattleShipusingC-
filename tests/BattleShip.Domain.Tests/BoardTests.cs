using BattleShip.Domain;
using Xunit;

namespace BattleShip.Domain.Tests;

public class BoardTests
{
    // ----- Đặt tàu -----

    [Fact]
    public void BoardIsTenByTen()
    {
        Assert.Equal(10, Board.Size);
    }

    [Theory]
    [InlineData(0, 0, Orientation.Horizontal)]
    [InlineData(0, 0, Orientation.Vertical)]
    [InlineData(5, 5, Orientation.Horizontal)]
    [InlineData(9, 5, Orientation.Horizontal)]
    [InlineData(5, 9, Orientation.Vertical)]
    public void CanPlaceShipInsideBoard(int row, int column, Orientation orientation)
    {
        var board = new Board();

        Assert.True(board.CanPlaceShip(5, new Cell(row, column), orientation));
    }

    [Theory]
    [InlineData(0, 6, Orientation.Horizontal)]   // vượt biên phải
    [InlineData(6, 0, Orientation.Vertical)]     // vượt biên dưới
    [InlineData(-1, 0, Orientation.Horizontal)]  // vượt biên trên
    [InlineData(0, -1, Orientation.Vertical)]    // vượt biên trái
    [InlineData(9, 8, Orientation.Horizontal)]
    public void CannotLeaveBoard(int row, int column, Orientation orientation)
    {
        var board = new Board();

        Assert.False(board.CanPlaceShip(5, new Cell(row, column), orientation));
    }

    [Fact]
    public void CannotOverlap()
    {
        var board = new Board();
        board.PlaceShip("A", 3, new Cell(0, 0), Orientation.Horizontal);

        Assert.False(board.CanPlaceShip(2, new Cell(0, 2), Orientation.Vertical));
    }

    [Fact]
    public void ShipsMayTouchByEdge()
    {
        var board = new Board();
        board.PlaceShip("A", 3, new Cell(0, 0), Orientation.Horizontal);

        Assert.True(board.CanPlaceShip(3, new Cell(1, 0), Orientation.Horizontal));
    }

    [Fact]
    public void ShipsMayTouchByCorner()
    {
        var board = new Board();
        board.PlaceShip("A", 2, new Cell(0, 0), Orientation.Horizontal);

        Assert.True(board.CanPlaceShip(2, new Cell(1, 2), Orientation.Horizontal));
    }

    [Fact]
    public void PlaceShipThrowsWhenPositionIsInvalid()
    {
        var board = new Board();

        Assert.Throws<InvalidOperationException>(
            () => board.PlaceShip("A", 5, new Cell(0, 8), Orientation.Horizontal));
        Assert.Empty(board.Ships);
    }

    [Fact]
    public void PlaceShipCoversExpectedCells()
    {
        var board = new Board();

        var ship = board.PlaceShip("A", 3, new Cell(2, 4), Orientation.Vertical);

        Assert.Equal(
            new[] { new Cell(2, 4), new Cell(3, 4), new Cell(4, 4) },
            ship.Cells.OrderBy(c => c.Row));
        Assert.Single(board.Ships);
    }

    [Fact]
    public void DefaultFleetHasFiveShipsWithRuleLengths()
    {
        Assert.Equal(new[] { 5, 4, 3, 3, 2 }, Fleet.Default.Select(s => s.Length));
        Assert.Equal(17, Fleet.Default.Sum(s => s.Length));
    }

    [Fact]
    public void FullDefaultFleetFitsOnBoard()
    {
        var board = new Board();
        var row = 0;
        foreach (var (name, length) in Fleet.Default)
        {
            board.PlaceShip(name, length, new Cell(row++, 0), Orientation.Horizontal);
        }

        Assert.Equal(Fleet.Default.Count, board.Ships.Count);
    }

    // ----- Bắn -----

    [Fact]
    public void ShootMissReturnsMiss()
    {
        var board = BoardWithShip(length: 2);

        Assert.Equal(ShotResult.Miss, board.Shoot(new Cell(5, 5)));
    }

    [Fact]
    public void ShootHitReturnsHit()
    {
        var board = BoardWithTwoShips();

        Assert.Equal(ShotResult.Hit, board.Shoot(new Cell(0, 0)));
    }

    [Fact]
    public void LastCellOfShipReturnsSunk()
    {
        var board = BoardWithTwoShips();

        board.Shoot(new Cell(0, 0));

        Assert.Equal(ShotResult.Sunk, board.Shoot(new Cell(0, 1)));
    }

    [Fact]
    public void LastCellOfLastShipReturnsGameOver()
    {
        var board = BoardWithShip(length: 2);

        board.Shoot(new Cell(0, 0));

        Assert.Equal(ShotResult.GameOver, board.Shoot(new Cell(0, 1)));
        Assert.True(board.AllShipsSunk);
    }

    [Fact]
    public void DuplicateShotRejected()
    {
        var board = BoardWithShip(length: 2);

        Assert.Equal(ShotResult.Hit, board.Shoot(new Cell(0, 0)));
        Assert.Equal(ShotResult.AlreadyShot, board.Shoot(new Cell(0, 0)));
    }

    [Fact]
    public void DuplicateMissIsAlsoRejected()
    {
        var board = BoardWithShip(length: 2);

        board.Shoot(new Cell(5, 5));

        Assert.Equal(ShotResult.AlreadyShot, board.Shoot(new Cell(5, 5)));
    }

    [Fact]
    public void DuplicateShotDoesNotChangeShipState()
    {
        var board = BoardWithTwoShips();

        board.Shoot(new Cell(0, 0));
        board.Shoot(new Cell(0, 0));

        Assert.Single(board.Ships[0].Hits);
        Assert.False(board.Ships[0].IsSunk);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public void ShootOutsideBoardThrows(int row, int column)
    {
        var board = BoardWithShip(length: 2);

        Assert.Throws<ArgumentOutOfRangeException>(() => board.Shoot(new Cell(row, column)));
    }

    [Fact]
    public void AllShipsSunkIsFalseOnEmptyBoard()
    {
        Assert.False(new Board().AllShipsSunk);
    }

    [Fact]
    public void WasShotTracksShots()
    {
        var board = BoardWithShip(length: 2);

        board.Shoot(new Cell(4, 4));

        Assert.True(board.WasShot(new Cell(4, 4)));
        Assert.False(board.WasShot(new Cell(4, 5)));
    }

    // ----- Trạng thái ô (phục vụ UI) -----

    [Fact]
    public void GetStateHidesShipsUnlessRevealed()
    {
        var board = BoardWithShip(length: 2);

        Assert.Equal(CellState.Unknown, board.GetState(new Cell(0, 0)));
        Assert.Equal(CellState.Ship, board.GetState(new Cell(0, 0), revealShips: true));
        Assert.Equal(CellState.Unknown, board.GetState(new Cell(5, 5), revealShips: true));
    }

    [Fact]
    public void GetStateReportsHitMissAndSunk()
    {
        var board = BoardWithTwoShips();

        board.Shoot(new Cell(0, 0));
        board.Shoot(new Cell(5, 5));

        Assert.Equal(CellState.Hit, board.GetState(new Cell(0, 0)));
        Assert.Equal(CellState.Miss, board.GetState(new Cell(5, 5)));

        board.Shoot(new Cell(0, 1));

        Assert.Equal(CellState.Sunk, board.GetState(new Cell(0, 0)));
        Assert.Equal(CellState.Sunk, board.GetState(new Cell(0, 1)));
    }

    // ----- Ship -----

    [Fact]
    public void ShipRejectsWrongNumberOfCells()
    {
        Assert.Throws<ArgumentException>(() => new Ship("A", 3, [new Cell(0, 0), new Cell(0, 1)]));
    }

    [Fact]
    public void ShipRegisterHitRejectsForeignCell()
    {
        var ship = new Ship("A", 2, [new Cell(0, 0), new Cell(0, 1)]);

        Assert.Throws<ArgumentException>(() => ship.RegisterHit(new Cell(5, 5)));
        Assert.Empty(ship.Hits);
    }

    // ----- Helpers -----

    private static Board BoardWithShip(int length)
    {
        var board = new Board();
        board.PlaceShip("A", length, new Cell(0, 0), Orientation.Horizontal);
        return board;
    }

    private static Board BoardWithTwoShips()
    {
        var board = new Board();
        board.PlaceShip("A", 2, new Cell(0, 0), Orientation.Horizontal);
        board.PlaceShip("B", 3, new Cell(2, 0), Orientation.Horizontal);
        return board;
    }
}
