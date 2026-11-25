using BattleShip.Application;
using BattleShip.Domain;
using Xunit;
using Xunit.Abstractions;

namespace BattleShip.Domain.Tests;

public class BotTests
{
    private readonly ITestOutputHelper _output;

    public BotTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly Dictionary<string, Func<Random, IBotStrategy>> Bots = new()
    {
        ["Easy"] = random => new EasyBot(random),
        ["Medium"] = random => new MediumBot(random),
        ["Hard"] = random => new HardBot(random),
    };

    public static TheoryData<string> BotNames => new() { "Easy", "Medium", "Hard" };

    // ----- Quy tắc chung cho mọi bot -----

    [Theory]
    [MemberData(nameof(BotNames))]
    public void BotFinishesGameWithoutRepeatingShots(string name)
    {
        for (var seed = 1; seed <= 5; seed++)
        {
            var board = RandomFleetBoard(new Random(seed));
            var bot = Bots[name](new Random(seed));

            var shots = PlayUntilFleetSunk(bot, board);

            Assert.True(board.AllShipsSunk);
            Assert.InRange(shots, Fleet.Default.Sum(s => s.Length), Board.Size * Board.Size);
        }
    }

    [Theory]
    [MemberData(nameof(BotNames))]
    public void BotThrowsWhenNoCellIsLeft(string name)
    {
        var board = RandomFleetBoard(new Random(1));
        foreach (var cell in AllCells())
        {
            board.Shoot(cell);
        }

        var bot = Bots[name](new Random(1));

        Assert.Throws<InvalidOperationException>(() => bot.ChooseTarget(board));
    }

    [Theory]
    [MemberData(nameof(BotNames))]
    public void BotChoiceDoesNotDependOnHiddenShipPositions(string name)
    {
        // Hai bàn có hạm đội đặt khác hẳn nhau nhưng lịch sử bắn công khai giống hệt
        // (toàn phát trượt) => bot cùng seed phải chọn cùng một ô.
        var left = new Board();
        var right = new Board();
        var row = 0;
        foreach (var (shipName, length) in Fleet.Default)
        {
            left.PlaceShip(shipName, length, new Cell(row, 0), Orientation.Horizontal);
            right.PlaceShip(shipName, length, new Cell(row, 5), Orientation.Horizontal);
            row++;
        }

        foreach (var shot in new[] { new Cell(7, 0), new Cell(7, 1), new Cell(8, 3), new Cell(9, 9) })
        {
            Assert.Equal(ShotResult.Miss, left.Shoot(shot));
            Assert.Equal(ShotResult.Miss, right.Shoot(shot));
        }

        var fromLeft = Bots[name](new Random(42)).ChooseTarget(left);
        var fromRight = Bots[name](new Random(42)).ChooseTarget(right);

        Assert.Equal(fromLeft, fromRight);
    }

    // ----- Medium: Hunt-Target -----

    [Fact]
    public void MediumShootsAdjacentCellAfterAHit()
    {
        for (var seed = 1; seed <= 20; seed++)
        {
            var board = BoardWithShip(new Cell(5, 4), 3, Orientation.Horizontal);
            board.Shoot(new Cell(5, 5));

            var target = new MediumBot(new Random(seed)).ChooseTarget(board);

            Assert.Contains(target, new Cell(5, 5).Neighbors4());
        }
    }

    [Fact]
    public void MediumExtendsALineOfTwoHits()
    {
        for (var seed = 1; seed <= 20; seed++)
        {
            var board = BoardWithShip(new Cell(5, 4), 4, Orientation.Horizontal);
            board.Shoot(new Cell(5, 5));
            board.Shoot(new Cell(5, 6));

            var target = new MediumBot(new Random(seed)).ChooseTarget(board);

            Assert.Contains(target, new[] { new Cell(5, 4), new Cell(5, 7) });
        }
    }

    [Fact]
    public void MediumKeepsTargetingAnotherShipAfterOneSinks()
    {
        for (var seed = 1; seed <= 20; seed++)
        {
            var board = new Board();
            board.PlaceShip("Small", 2, new Cell(0, 0), Orientation.Horizontal);
            board.PlaceShip("Big", 3, new Cell(5, 5), Orientation.Horizontal);

            board.Shoot(new Cell(5, 6));                         // trúng tàu lớn
            board.Shoot(new Cell(0, 0));
            Assert.Equal(ShotResult.Sunk, board.Shoot(new Cell(0, 1))); // chìm tàu nhỏ

            var target = new MediumBot(new Random(seed)).ChooseTarget(board);

            Assert.Contains(target, new Cell(5, 6).Neighbors4());
        }
    }

    // ----- Hard: heatmap + ràng buộc + BFS -----

    [Fact]
    public void HardShootsNextToAHit()
    {
        for (var seed = 1; seed <= 10; seed++)
        {
            var board = BoardWithShip(new Cell(5, 4), 3, Orientation.Horizontal);
            board.Shoot(new Cell(5, 5));

            var target = new HardBot(new Random(seed)).ChooseTarget(board);

            Assert.Contains(target, new Cell(5, 5).Neighbors4());
        }
    }

    [Fact]
    public void HardExtendsALineOfTwoHits()
    {
        for (var seed = 1; seed <= 10; seed++)
        {
            var board = BoardWithShip(new Cell(5, 4), 4, Orientation.Horizontal);
            board.Shoot(new Cell(5, 5));
            board.Shoot(new Cell(5, 6));

            var target = new HardBot(new Random(seed)).ChooseTarget(board);

            Assert.Contains(target, new[] { new Cell(5, 4), new Cell(5, 7) });
        }
    }

    [Fact]
    public void HardNeverPicksACellThatCannotHoldAShip()
    {
        // Ô (5,5) bị bao quanh bởi phát trượt ở 3 phía; ô kế tiếp hợp lý duy nhất để mở rộng là (5,6).
        var board = BoardWithShip(new Cell(5, 5), 3, Orientation.Horizontal);
        board.Shoot(new Cell(5, 5));
        board.Shoot(new Cell(4, 5));
        board.Shoot(new Cell(6, 5));
        board.Shoot(new Cell(5, 4));

        var target = new HardBot(new Random(1)).ChooseTarget(board);

        Assert.Equal(new Cell(5, 6), target);
    }

    [Fact]
    public void HardIgnoresSunkShipCellsWhenTargeting()
    {
        var board = new Board();
        board.PlaceShip("Small", 2, new Cell(0, 0), Orientation.Horizontal);
        board.PlaceShip("Big", 3, new Cell(5, 5), Orientation.Horizontal);
        board.Shoot(new Cell(0, 0));
        board.Shoot(new Cell(0, 1)); // chìm
        board.Shoot(new Cell(5, 6)); // trúng tàu lớn

        var target = new HardBot(new Random(1)).ChooseTarget(board);

        Assert.Contains(target, new Cell(5, 6).Neighbors4());
    }

    // ----- Độ khó tăng dần -----

    [Fact]
    public void HarderBotsNeedFewerShotsOnAverage()
    {
        const int games = 40;
        var averages = new Dictionary<string, double>();

        foreach (var (name, create) in Bots)
        {
            var total = 0;
            for (var seed = 1; seed <= games; seed++)
            {
                var board = RandomFleetBoard(new Random(1000 + seed));
                total += PlayUntilFleetSunk(create(new Random(seed)), board);
            }

            averages[name] = (double)total / games;
        }

        var summary = $"Trung bình số phát bắn ({games} ván): "
            + $"Easy={averages["Easy"]:F1}, Medium={averages["Medium"]:F1}, Hard={averages["Hard"]:F1}";
        _output.WriteLine(summary);

        Assert.True(averages["Hard"] < averages["Medium"] && averages["Medium"] < averages["Easy"], summary);
    }

    // ----- Helpers -----

    private static IEnumerable<Cell> AllCells()
    {
        for (var row = 0; row < Board.Size; row++)
        {
            for (var column = 0; column < Board.Size; column++)
            {
                yield return new Cell(row, column);
            }
        }
    }

    private static Board BoardWithShip(Cell start, int length, Orientation orientation)
    {
        var board = new Board();
        board.PlaceShip("Ship", length, start, orientation);
        return board;
    }

    private static Board RandomFleetBoard(Random random)
    {
        var board = new Board();
        foreach (var (name, length) in Fleet.Default)
        {
            while (true)
            {
                var start = new Cell(random.Next(Board.Size), random.Next(Board.Size));
                var orientation = random.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical;
                if (board.CanPlaceShip(length, start, orientation))
                {
                    board.PlaceShip(name, length, start, orientation);
                    break;
                }
            }
        }

        return board;
    }

    private static int PlayUntilFleetSunk(IBotStrategy bot, Board board)
    {
        var shots = 0;
        while (!board.AllShipsSunk)
        {
            var target = bot.ChooseTarget(board);

            Assert.True(target.IsInside(), $"Bot bắn ngoài bàn cờ: {target}");
            Assert.False(board.WasShot(target), $"Bot bắn lại ô đã bắn: {target}");

            var result = board.Shoot(target);
            bot.ProcessResult(target, result);
            shots++;

            Assert.True(shots <= Board.Size * Board.Size, "Bot không kết thúc được ván.");
        }

        return shots;
    }
}
