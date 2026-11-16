using BattleShip.Domain;

namespace BattleShip.Application;

/// <summary>
/// Bot Hard: Probability Heatmap + Constraint Placement (backtracking) + Hunt-Target + BFS.
/// <list type="number">
/// <item>Heatmap: với mỗi tàu còn sống, đếm mọi vị trí đặt còn hợp lệ trên bàn (không đè ô trượt/ô tàu đã chìm)
/// và cộng điểm cho từng ô chưa bắn mà vị trí đó đi qua.</item>
/// <item>Ràng buộc/backtracking: khi còn ô trúng chưa chìm, một vị trí chỉ được tính nếu các tàu còn lại
/// vẫn có cách đặt để giải thích toàn bộ ô trúng còn lại.</item>
/// <item>Vị trí đi qua ô trúng được nhân trọng số lớn (chế độ Target).</item>
/// <item>BFS gom các ô trúng thành vùng và lấy ô biên; ở chế độ Target chỉ chọn trong các ô biên.</item>
/// </list>
/// Bot chỉ dùng thông tin công khai: trạng thái ô đã bắn và độ dài các tàu đã chìm.
/// </summary>
public sealed class ProbabilityBot : IBotStrategy
{
    /// <summary>Điểm cộng cho mỗi ô trúng mà một vị trí đặt tàu đi qua.</summary>
    private const int HitWeight = 100;

    private readonly Random _random;

    public ProbabilityBot(Random? random = null)
    {
        _random = random ?? new Random();
    }

    public Cell ChooseTarget(Board enemyBoard)
    {
        var unshot = PublicBoardInfo.UnshotCells(enemyBoard);
        if (unshot.Count == 0)
        {
            throw new InvalidOperationException("Không còn ô nào để bắn.");
        }

        var hits = PublicBoardInfo.UnsunkHits(enemyBoard);
        var lengths = PublicBoardInfo.RemainingShipLengths(enemyBoard);
        var states = SnapshotStates(enemyBoard);

        var scores = BuildHeatmap(states, hits, lengths);
        var candidates = hits.Count > 0 ? TargetCandidates(enemyBoard, hits, scores) : unshot;

        return PickBest(candidates, scores) ?? PickBest(unshot, scores) ?? unshot[_random.Next(unshot.Count)];
    }

    public void ProcessResult(Cell target, ShotResult result)
    {
        // Bot Hard suy ra mọi thứ từ bàn cờ trong ChooseTarget, không giữ trạng thái riêng.
    }

    private static CellState[,] SnapshotStates(Board board)
    {
        var states = new CellState[Board.Size, Board.Size];
        foreach (var cell in PublicBoardInfo.AllCells())
        {
            states[cell.Row, cell.Column] = board.GetState(cell);
        }

        return states;
    }

    private int[,] BuildHeatmap(CellState[,] states, List<Cell> hits, List<int> lengths)
    {
        var scores = new int[Board.Size, Board.Size];
        var hitSet = hits.ToHashSet();

        for (var index = 0; index < lengths.Count; index++)
        {
            var length = lengths[index];

            // Nếu đã xét độ dài này (tàu cùng cỡ), chỉ cần tính một lần rồi nhân số lượng.
            if (lengths.IndexOf(length) != index)
            {
                continue;
            }

            var copies = lengths.Count(l => l == length);
            var others = new List<int>(lengths);
            others.Remove(length);

            foreach (var orientation in Enum.GetValues<Orientation>())
            {
                foreach (var start in PublicBoardInfo.AllCells())
                {
                    var cells = Board.GetCells(length, start, orientation).ToArray();
                    if (!IsPlaceable(states, cells))
                    {
                        continue;
                    }

                    var covered = cells.Count(hitSet.Contains);
                    if (hitSet.Count > 0 && !CanExplainHits(states, hitSet.Except(cells).ToHashSet(), others, cells.ToHashSet()))
                    {
                        continue;
                    }

                    var weight = (1 + HitWeight * covered) * copies;
                    foreach (var cell in cells)
                    {
                        if (states[cell.Row, cell.Column] == CellState.Unknown)
                        {
                            scores[cell.Row, cell.Column] += weight;
                        }
                    }
                }
            }
        }

        return scores;
    }

    /// <summary>Một vị trí hợp lệ khi nằm trong bàn và không đè ô trượt hoặc ô của tàu đã chìm.</summary>
    private static bool IsPlaceable(CellState[,] states, IEnumerable<Cell> cells)
    {
        return cells.All(cell =>
            cell.IsInside()
            && states[cell.Row, cell.Column] is CellState.Unknown or CellState.Hit);
    }

    /// <summary>
    /// Backtracking: các tàu <paramref name="lengths"/> còn lại có cách đặt (không chồng nhau, không chồng
    /// <paramref name="occupied"/>) để phủ toàn bộ <paramref name="uncovered"/> hay không.
    /// Luôn xử lý ô trúng chưa được giải thích đầu tiên nên số nhánh rất nhỏ.
    /// </summary>
    private static bool CanExplainHits(
        CellState[,] states,
        HashSet<Cell> uncovered,
        List<int> lengths,
        HashSet<Cell> occupied)
    {
        if (uncovered.Count == 0)
        {
            return true;
        }

        if (lengths.Sum() < uncovered.Count)
        {
            return false;
        }

        var target = uncovered.OrderBy(c => c.Row).ThenBy(c => c.Column).First();

        foreach (var length in lengths.Distinct())
        {
            var remaining = new List<int>(lengths);
            remaining.Remove(length);

            foreach (var orientation in Enum.GetValues<Orientation>())
            {
                for (var offset = 0; offset < length; offset++)
                {
                    var start = orientation == Orientation.Horizontal
                        ? new Cell(target.Row, target.Column - offset)
                        : new Cell(target.Row - offset, target.Column);

                    var cells = Board.GetCells(length, start, orientation).ToArray();
                    if (!IsPlaceable(states, cells) || cells.Any(occupied.Contains))
                    {
                        continue;
                    }

                    var stillUncovered = uncovered.Except(cells).ToHashSet();
                    var nowOccupied = new HashSet<Cell>(occupied);
                    nowOccupied.UnionWith(cells);

                    if (CanExplainHits(states, stillUncovered, remaining, nowOccupied))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>Chế độ Target: các ô biên (chưa bắn, sát vùng trúng) có điểm xác suất &gt; 0.</summary>
    private static List<Cell> TargetCandidates(Board board, List<Cell> hits, int[,] scores)
    {
        var frontier = new HashSet<Cell>();
        foreach (var cluster in PublicBoardInfo.GroupIntoClusters(hits))
        {
            frontier.UnionWith(PublicBoardInfo.Frontier(board, cluster));
        }

        return frontier.Where(cell => scores[cell.Row, cell.Column] > 0).ToList();
    }

    private Cell? PickBest(IReadOnlyList<Cell> candidates, int[,] scores)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var best = candidates.Max(cell => scores[cell.Row, cell.Column]);
        if (best <= 0)
        {
            return null;
        }

        var top = candidates.Where(cell => scores[cell.Row, cell.Column] == best).ToList();
        return top[_random.Next(top.Count)];
    }
}
