using System.Windows.Controls;
using System.Windows.Input;
using BattleShip.Wpf.ViewModels;

namespace BattleShip.Wpf.Views
{
    public partial class FleetPlacementView : UserControl
    {
        public FleetPlacementView()
        {
            InitializeComponent();
            Loaded += (_, _) => Focus();
        }

        private FleetPlacementViewModel? ViewModel => DataContext as FleetPlacementViewModel;

        /// <summary>Đổi toạ độ chuột thành (hàng, cột) của lưới 10x10.</summary>
        private (int Row, int Column)? CellAt(MouseEventArgs e)
        {
            var position = e.GetPosition(PlacementHit);
            var width = PlacementHit.ActualWidth;
            var height = PlacementHit.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            var column = (int)(position.X / width * 10);
            var row = (int)(position.Y / height * 10);
            return row is >= 0 and < 10 && column is >= 0 and < 10 ? (row, column) : null;
        }

        private void OnGridMouseMove(object sender, MouseEventArgs e)
        {
            if (CellAt(e) is { } cell)
            {
                ViewModel?.Hover(cell.Row, cell.Column);
            }
        }

        private void OnGridMouseLeave(object sender, MouseEventArgs e) => ViewModel?.ClearHover();

        private void OnGridLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (CellAt(e) is { } cell)
            {
                ViewModel?.Hover(cell.Row, cell.Column);
                ViewModel?.PlaceAtHover();
            }
        }

        private void OnGridRightClick(object sender, MouseButtonEventArgs e)
        {
            if (CellAt(e) is { } cell)
            {
                ViewModel?.PickUpAt(cell.Row, cell.Column);
                ViewModel?.Hover(cell.Row, cell.Column);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.R && ViewModel is { } vm)
            {
                vm.RotateCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
