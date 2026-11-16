namespace BattleShip.Domain;

/// <summary>Tọa độ một ô trên bàn cờ (hàng, cột), tính từ 0.</summary>
public readonly record struct Cell(int Row, int Column)
{
    public bool IsInside(int size = Board.Size)
    {
        return Row >= 0 && Row < size && Column >= 0 && Column < size;
    }

    /// <summary>Các ô liền kề theo 4 hướng (trên, dưới, trái, phải) nằm trong bàn cờ.</summary>
    public IEnumerable<Cell> Neighbors4(int size = Board.Size)
    {
        var candidates = new[]
        {
            new Cell(Row - 1, Column),
            new Cell(Row + 1, Column),
            new Cell(Row, Column - 1),
            new Cell(Row, Column + 1),
        };

        foreach (var candidate in candidates)
        {
            if (candidate.IsInside(size))
            {
                yield return candidate;
            }
        }
    }
}
