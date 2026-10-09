namespace BattleShip.Domain;

public readonly record struct Cell(int Row, int Column)
{
    public bool IsInside(int size = 10) => Row >= 0 && Row < size && Column >= 0 && Column < size;
    public IEnumerable<Cell> Neighbors4(int size = 10)
    { foreach (var (r,c) in new[]{(Row-1,Column),(Row+1,Column),(Row,Column-1),(Row,Column+1)}) if(new Cell(r,c).IsInside(size)) yield return new(r,c); }
}
public enum Orientation { Horizontal, Vertical }
public enum CellState { Unknown, Ship, Hit, Miss, Sunk }
public enum ShotResult { Hit, Miss, Sunk, AlreadyShot, GameOver }
public enum GameStatus { Setup, InProgress, PlayerWon, BotWon }

public sealed class Ship
{
    private readonly HashSet<Cell> _cells;
    private readonly HashSet<Cell> _hits = [];
    public string Name { get; } public int Length { get; }
    public IReadOnlySet<Cell> Cells => _cells; public IReadOnlySet<Cell> Hits => _hits;
    public bool IsSunk => _hits.Count == Length;
    public Ship(string name, int length, IEnumerable<Cell> cells) { Name=name; Length=length; _cells=cells.ToHashSet(); if(_cells.Count!=length) throw new ArgumentException("Invalid ship cells"); }
    public bool Contains(Cell cell) => _cells.Contains(cell);
    public void RegisterHit(Cell cell) { if(Contains(cell)) _hits.Add(cell); }
}

public sealed class Board
{
    public const int Size = 10; private readonly List<Ship> _ships=[]; private readonly HashSet<Cell> _shots=[];
    public IReadOnlyList<Ship> Ships=>_ships; public IReadOnlySet<Cell> Shots=>_shots;
    public bool AllShipsSunk=>_ships.Count>0 && _ships.All(s=>s.IsSunk);
    public static IEnumerable<Cell> GetCells(int length, Cell start, Orientation orientation) { for(var i=0;i<length;i++) yield return orientation==Orientation.Horizontal?new(start.Row,start.Column+i):new(start.Row+i,start.Column); }
    public bool CanPlaceShip(int length, Cell start, Orientation orientation) { var cells=GetCells(length,start,orientation); return cells.All(c=>c.IsInside() && _ships.All(s=>!s.Contains(c))); }
    // Used by a bot's probability model: only public shot information is considered.
    public bool CanFitUnknownShip(int length, Cell start, Orientation orientation) => GetCells(length,start,orientation).All(c=>c.IsInside() && !_shots.Contains(c));
    public Ship PlaceShip(string name,int length,Cell start,Orientation orientation) { var cells=GetCells(length,start,orientation).ToArray(); if(!CanPlaceShip(length,start,orientation)) throw new InvalidOperationException("Invalid ship position"); var ship=new Ship(name,length,cells); _ships.Add(ship); return ship; }
    public ShotResult Shoot(Cell cell) { if(!cell.IsInside()) throw new ArgumentOutOfRangeException(nameof(cell)); if(!_shots.Add(cell)) return ShotResult.AlreadyShot; var ship=_ships.FirstOrDefault(s=>s.Contains(cell)); if(ship is null)return ShotResult.Miss; ship.RegisterHit(cell); return ship.IsSunk?(_ships.All(s=>s.IsSunk)?ShotResult.GameOver:ShotResult.Sunk):ShotResult.Hit; }
    public bool WasShot(Cell cell)=>_shots.Contains(cell);
    public CellState GetState(Cell cell,bool revealShips=false) { var ship=_ships.FirstOrDefault(s=>s.Contains(cell)); if(ship is not null&&ship.Hits.Contains(cell))return ship.IsSunk?CellState.Sunk:CellState.Hit; if(_shots.Contains(cell))return CellState.Miss; return revealShips&&ship is not null?CellState.Ship:CellState.Unknown; }
}
public static class Fleet { public static readonly IReadOnlyList<(string Name,int Length)> Default=[("Carrier",5),("Battleship",4),("Cruiser",3),("Submarine",3),("Destroyer",2)]; }
