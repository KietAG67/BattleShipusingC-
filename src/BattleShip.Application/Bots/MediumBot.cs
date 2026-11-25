using BattleShip.Domain;

namespace BattleShip.Application;

/// <summary>
/// Bot Medium: Hunt-Target.
/// Hunt: bắn ngẫu nhiên. Target: khi có ô trúng mà tàu chưa chìm thì bắn quanh nó,
/// ưu tiên kéo dài hàng ô trúng thẳng hàng. Trạng thái được suy ra từ bàn cờ mỗi lượt,
/// nên một tàu chìm không làm bot quên các ô trúng của tàu khác.
/// </summary>
public sealed class MediumBot : IBotStrategy
{
    private readonly Random _random;
    private readonly EasyBot _hunt;

    public MediumBot(Random? random = null)
    {
        _random = random ?? new Random();
        _hunt = new EasyBot(_random);
    }

    public Cell ChooseTarget(Board enemyBoard)
    {
        var hits = PublicBoardInfo.UnsunkHits(enemyBoard);
        if (hits.Count > 0)
        {
            var targets = LineExtensions(enemyBoard, hits);
            if (targets.Count == 0)
            {
                targets = AdjacentTargets(enemyBoard, hits);
            }

            if (targets.Count > 0)
            {
                return targets[_random.Next(targets.Count)];
            }
        }

        return _hunt.ChooseTarget(enemyBoard);
    }

    public void ProcessResult(Cell target, ShotResult result)
    {
        // Trạng thái được đọc lại từ bàn cờ trong ChooseTarget.
    }

    /// <summary>Ô chưa bắn ở hai đầu của các hàng ≥2 ô trúng liên tiếp.</summary>
    private static List<Cell> LineExtensions(Board board, IReadOnlyList<Cell> hits)
    {
        var hitSet = hits.ToHashSet();
        var targets = new HashSet<Cell>();
        (int Row, int Column)[] directions = [(0, 1), (0, -1), (1, 0), (-1, 0)];

        foreach (var hit in hits)
        {
            foreach (var (dRow, dColumn) in directions)
            {
                var next = new Cell(hit.Row + dRow, hit.Column + dColumn);
                if (!hitSet.Contains(next))
                {
                    continue;
                }

                var end = next;
                while (hitSet.Contains(end))
                {
                    end = new Cell(end.Row + dRow, end.Column + dColumn);
                }

                if (end.IsInside() && !board.WasShot(end))
                {
                    targets.Add(end);
                }
            }
        }

        return targets.ToList();
    }

    private static List<Cell> AdjacentTargets(Board board, IReadOnlyList<Cell> hits)
    {
        return hits
            .SelectMany(hit => hit.Neighbors4())
            .Where(cell => !board.WasShot(cell))
            .Distinct()
            .ToList();
    }
}
