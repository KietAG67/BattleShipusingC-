using System.Collections.ObjectModel;
using BattleShip.Domain;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    /// <summary>Dữ liệu hiển thị của một bàn cờ 10x10: 100 ô + các hình tàu. Dùng cho Fleet Placement và Battle.</summary>
    public sealed class BoardViewModel : ViewModelBase
    {
        public BoardViewModel(Action<CellViewModel>? onCellClick = null)
        {
            Cells = new ObservableCollection<CellViewModel>();
            Ships = new ObservableCollection<ShipSpriteViewModel>();

            for (var row = 0; row < Board.Size; row++)
            {
                for (var column = 0; column < Board.Size; column++)
                {
                    Cells.Add(new CellViewModel(row, column, onCellClick));
                }
            }
        }

        /// <summary>Thứ tự hàng trước rồi đến cột (khớp UniformGrid 10x10).</summary>
        public ObservableCollection<CellViewModel> Cells { get; }

        public ObservableCollection<ShipSpriteViewModel> Ships { get; }

        public CellViewModel GetCell(int row, int column) => Cells[(row * Board.Size) + column];

        /// <summary>Cập nhật trạng thái các ô theo Board trong Domain. revealShips=false cho bàn cờ của bot.</summary>
        public void Refresh(Board board, bool revealShips)
        {
            foreach (var cell in Cells)
            {
                cell.State = board.GetState(cell.Cell, revealShips);
            }
        }

        /// <summary>Thêm hình một tàu của Domain lên lưới (hướng suy ra từ các ô của tàu).</summary>
        public void AddShipSprite(Ship ship)
        {
            var minRow = ship.Cells.Min(c => c.Row);
            var minColumn = ship.Cells.Min(c => c.Column);
            var vertical = ship.Cells.Select(c => c.Column).Distinct().Count() == 1;

            Ships.Add(CreateSprite(ship.Name, minRow, minColumn, ship.Length, vertical));
        }

        public static ShipSpriteViewModel CreateSprite(string shipName, int row, int column, int length, bool vertical, double opacity = 1.0)
            => new(
                row,
                column,
                vertical ? length : 1,
                vertical ? 1 : length,
                ShipArt.Get(shipName, vertical),
                opacity);

        public void SetPreview(IEnumerable<Cell> cells, bool valid)
        {
            ClearPreview();
            foreach (var cell in cells)
            {
                if (cell.IsInside())
                {
                    GetCell(cell.Row, cell.Column).Preview = valid ? PreviewKind.Valid : PreviewKind.Invalid;
                }
            }
        }

        public void ClearPreview()
        {
            foreach (var cell in Cells)
            {
                cell.Preview = PreviewKind.None;
            }
        }
    }
}
