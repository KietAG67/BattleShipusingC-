namespace BattleShip.Domain;

/// <summary>Một con tàu: tập ô nó chiếm và tập ô đã bị bắn trúng.</summary>
public sealed class Ship
{
    private readonly HashSet<Cell> _cells;
    private readonly HashSet<Cell> _hits = [];

    public Ship(string name, int length, IEnumerable<Cell> cells)
    {
        Name = name;
        Length = length;
        _cells = cells.ToHashSet();

        if (_cells.Count != length)
        {
            throw new ArgumentException("Invalid ship cells");
        }
    }

    public string Name { get; }

    public int Length { get; }

    public IReadOnlySet<Cell> Cells => _cells;

    public IReadOnlySet<Cell> Hits => _hits;

    public bool IsSunk => _hits.Count == Length;

    public bool Contains(Cell cell) => _cells.Contains(cell);

    /// <summary>Ghi nhận một phát trúng. Ném lỗi nếu ô không thuộc tàu này.</summary>
    public void RegisterHit(Cell cell)
    {
        if (!Contains(cell))
        {
            throw new ArgumentException("Cell does not belong to this ship.", nameof(cell));
        }

        _hits.Add(cell);
    }
}
