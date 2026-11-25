using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BattleShip.Wpf.Effects;
using BattleShip.Wpf.ViewModels;

namespace BattleShip.Wpf.Views
{
    public partial class BattleView : UserControl
    {
        // Vị trí hai lưới trong ảnh Battle.jpg (pixel) để tính điểm rơi của đạn.
        private const double PlayerBoardX = 189;
        private const double EnemyBoardX = 590;
        private const double BoardY = 274;
        private const double BoardWidth = 324.5;
        private const double BoardHeight = 323;
        private const double FlightSeconds = 0.6;

        public BattleView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += (_, _) => Focus();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is BattleViewModel oldVm)
            {
                oldVm.PlayMissileAsync = null;
            }

            if (e.NewValue is BattleViewModel newVm)
            {
                newVm.PlayMissileAsync = PlayMissileAsync;
            }
        }

        /// <summary>Đạn bay từ ngoài màn hình tới đúng ô: người chơi bắn từ góc dưới trái, bot bắn từ góc trên phải.</summary>
        private Task PlayMissileAsync(bool fromPlayer, int row, int column)
        {
            var boardX = fromPlayer ? EnemyBoardX : PlayerBoardX;
            var target = new Point(
                boardX + ((column + 0.5) * BoardWidth / 10),
                BoardY + ((row + 0.5) * BoardHeight / 10));
            var start = fromPlayer ? new Point(120, 800) : new Point(1000, -40);

            return MissileFlight.PlayAsync(FxCanvas, start, target, FlightSeconds);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not BattleViewModel vm)
            {
                return;
            }

            if (e.Key is Key.Enter or Key.Space)
            {
                if (vm.FireCommand.CanExecute(null))
                {
                    vm.FireCommand.Execute(null);
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                var answer = MessageBox.Show(
                    "Do you want to surrender and end this match?",
                    "BattleShip",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer == MessageBoxResult.Yes)
                {
                    vm.Surrender();
                }

                e.Handled = true;
            }
        }
    }
}
