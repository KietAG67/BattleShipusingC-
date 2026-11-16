using BattleShip.Domain;

namespace BattleShip.Application;

/// <summary>
/// Các truy vấn chỉ dựa trên thông tin công khai của bàn đối thủ, dùng chung cho mọi bot.
/// Không hàm nào ở đây đọc vị trí của tàu chưa chìm.
/// </summary>
internal static class PublicBoardInfo
{
    public static IEnumerable<Cell> AllCells()
    {
        for (var row = 0; row < Board.Size; row++)
        {
            for (var column = 0; column < Board.Size; column++)
            {
                yield return new Cell(row, column);
            }
        }
    }

    public static List<Cell> UnshotCells(Board board)
    {
        return AllCells().Where(cell => !board.WasShot(cell)).ToList();
    }

    /// <summary>Các ô đã bắn trúng nhưng tàu chưa chìm.</summary>
    public static List<Cell> UnsunkHits(Board board)
    {
        return AllCells().Where(cell => board.GetState(cell) == CellState.Hit).ToList();
    }

    /// <summary>
    /// Độ dài các tàu còn sống = hạm đội mặc định trừ các tàu đã chìm
    /// (tàu chìm được thông báo công khai nên bot biết độ dài của chúng).
    /// </summary>
    public static List<int> RemainingShipLengths(Board board)
    {
        var remaining = Fleet.Default.Select(ship => ship.Length).ToList();

        foreach (var sunk in board.Ships.Where(ship => ship.IsSunk))
        {
            remaining.Remove(sunk.Length);
        }

        return remaining;
    }

    /// <summary>Gom các ô trúng liền kề (4 hướng) thành từng vùng bằng BFS.</summary>
    public static List<List<Cell>> GroupIntoClusters(IReadOnlyCollection<Cell> hits)
    {
        var pending = new HashSet<Cell>(hits);
        var clusters = new List<List<Cell>>();

        while (pending.Count > 0)
        {
            var start = pending.First();
            pending.Remove(start);

            var cluster = new List<Cell>();
            var queue = new Queue<Cell>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                cluster.Add(current);

                foreach (var neighbor in current.Neighbors4())
                {
                    if (pending.Remove(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    /// <summary>Các ô chưa bắn nằm sát (4 hướng) vùng trúng: "ô biên" của vùng.</summary>
    public static HashSet<Cell> Frontier(Board board, IEnumerable<Cell> cluster)
    {
        var frontier = new HashSet<Cell>();

        foreach (var cell in cluster)
        {
            foreach (var neighbor in cell.Neighbors4())
            {
                if (!board.WasShot(neighbor))
                {
                    frontier.Add(neighbor);
                }
            }
        }

        return frontier;
    }
}
