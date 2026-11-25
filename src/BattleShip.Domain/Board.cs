namespace BattleShip.Domain;

/// <summary>Bàn cờ 10x10: giữ các tàu đã đặt và các ô đã bị bắn.</summary>
public sealed class Board
{
    public const int Size = 10;

    private readonly List<Ship> _ships = [];
    private readonly HashSet<Cell> _shots = [];

    public IReadOnlyList<Ship> Ships => _ships;

    public IReadOnlySet<Cell> Shots => _shots;

    public bool AllShipsSunk => _ships.Count > 0 && _ships.All(ship => ship.IsSunk);

    public static IEnumerable<Cell> GetCells(int length, Cell start, Orientation orientation) ///hàm này tính toán và trả về danh sách các ô (Cell) mà con tàu sẽ chiếm giữ.
    {
        for (var i = 0; i < length; i++)
        {
            yield return orientation == Orientation.Horizontal
                ? new Cell(start.Row, start.Column + i)
                : new Cell(start.Row + i, start.Column);
        }
    }

    public bool CanPlaceShip(int length, Cell start, Orientation orientation) ///Kiểm tra xem có hợp lệ để đặt tàu tại vị trí đó không
    {
        return GetCells(length, start, orientation)
            .All(cell => cell.IsInside() && _ships.All(ship => !ship.Contains(cell)));
    }

    // Dùng cho mô hình xác suất của bot: chỉ xét thông tin bắn công khai.
    public bool CanFitUnknownShip(int length, Cell start, Orientation orientation)
    {
        return GetCells(length, start, orientation)
            .All(cell => cell.IsInside() && !_shots.Contains(cell));
    }

    public Ship PlaceShip(string name, int length, Cell start, Orientation orientation) ///g: Thực hiện đặt tàu lên bàn cờ. Nếu vị trí không hợp lệ (!CanPlaceShip), nó sẽ ném ra lỗi InvalidOperationException. Nếu hợp lệ, nó khởi tạo đối tượng Ship mới và thêm vào danh sách _ships.
    {
        if (!CanPlaceShip(length, start, orientation))
        {
            throw new InvalidOperationException("Invalid ship position");
        }

        var ship = new Ship(name, length, GetCells(length, start, orientation).ToArray());
        _ships.Add(ship);
        return ship;
    }

    public ShotResult Shoot(Cell cell) //: Xử lý toàn bộ logic khi có một phát bắn vào ô cell
    {
        if (!cell.IsInside())
        {
            throw new ArgumentOutOfRangeException(nameof(cell));
        }

        if (!_shots.Add(cell))
        {
            return ShotResult.AlreadyShot;
        }

        var ship = _ships.FirstOrDefault(s => s.Contains(cell));
        if (ship is null)
        {
            return ShotResult.Miss;
        }

        ship.RegisterHit(cell);
        if (!ship.IsSunk)
        {
            return ShotResult.Hit;
        }

        return _ships.All(s => s.IsSunk) ? ShotResult.GameOver : ShotResult.Sunk;
    }

    public bool WasShot(Cell cell) => _shots.Contains(cell);

    public CellState GetState(Cell cell, bool revealShips = false) ///Phục vụ cho tầng Giao diện (UI) biết cần phải vẽ màu/icon gì ở ô cell
    {
        var ship = _ships.FirstOrDefault(s => s.Contains(cell));

        if (ship is not null && ship.Hits.Contains(cell))
        {
            return ship.IsSunk ? CellState.Sunk : CellState.Hit;
        }

        if (_shots.Contains(cell))
        {
            return CellState.Miss;
        }

        return revealShips && ship is not null ? CellState.Ship : CellState.Unknown;
        ///Khi vẽ bàn cờ của người chơi: truyền revealShips = true để thấy được vị trí tàu của mình (CellState.Ship).
        ///Khi vẽ bàn cờ của đối thủ: truyền revealShips = false để giấu các tàu chưa bị bắn trúng (CellState.Unknown), tránh lộ vị trí tàu địch.
    }
}
