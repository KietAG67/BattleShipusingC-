using BattleShip.Domain;

namespace BattleShip.Application;

/// <summary>Bot Easy: bắn ngẫu nhiên vào các ô chưa bắn.</summary>
public sealed class RandomBot : IBotStrategy
{
    private readonly Random _random;

    public RandomBot(Random? random = null)
    {
        _random = random ?? new Random();
    }

    public Cell ChooseTarget(Board enemyBoard)
    {
        var candidates = PublicBoardInfo.UnshotCells(enemyBoard);
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("Không còn ô nào để bắn.");
        }

        return candidates[_random.Next(candidates.Count)];
    }

    public void ProcessResult(Cell target, ShotResult result)
    {
        // Bot ngẫu nhiên không cần ghi nhớ kết quả.
    }

    public static IEnumerable<Cell> Unshot(Board board)
    {
        return PublicBoardInfo.UnshotCells(board);
    }
}
