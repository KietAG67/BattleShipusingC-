using BattleShip.Domain;
using Xunit;

namespace BattleShip.Domain.Tests;

public class CellTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(10, 0, false)]
    [InlineData(0, 10, false)]
    public void IsInsideChecksTenByTenBounds(int row, int column, bool expected)
    {
        Assert.Equal(expected, new Cell(row, column).IsInside());
    }

    [Fact]
    public void CenterCellHasFourNeighbors()
    {
        var neighbors = new Cell(5, 5).Neighbors4().ToHashSet();

        Assert.Equal(
            new HashSet<Cell> { new(4, 5), new(6, 5), new(5, 4), new(5, 6) },
            neighbors);
    }

    [Fact]
    public void CornerCellHasTwoNeighbors()
    {
        var neighbors = new Cell(0, 0).Neighbors4().ToHashSet();

        Assert.Equal(new HashSet<Cell> { new(1, 0), new(0, 1) }, neighbors);
    }

    [Fact]
    public void EdgeCellNeighborsStayInsideBoard()
    {
        Assert.All(new Cell(9, 4).Neighbors4(), cell => Assert.True(cell.IsInside()));
        Assert.Equal(3, new Cell(9, 4).Neighbors4().Count());
    }
}
