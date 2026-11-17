using System.Windows.Media;

namespace BattleShip.Wpf.ViewModels
{
    /// <summary>Hình một con tàu đặt trên lưới (vị trí tính theo ô: Row/Column/RowSpan/ColumnSpan).</summary>
    public sealed class ShipSpriteViewModel
    {
        public ShipSpriteViewModel(int row, int column, int rowSpan, int columnSpan, ImageSource image, double opacity = 1.0)
        {
            Row = row;
            Column = column;
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
            Image = image;
            Opacity = opacity;
        }

        public int Row { get; }

        public int Column { get; }

        public int RowSpan { get; }

        public int ColumnSpan { get; }

        public ImageSource Image { get; }

        public double Opacity { get; }
    }
}
