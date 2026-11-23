using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BattleShip.Wpf.Views
{
    /// <summary>Bàn cờ 10x10 dùng lại cho Fleet Placement và Battle. DataContext là BoardViewModel.</summary>
    public partial class BoardGridView : UserControl
    {
        public static readonly DependencyProperty BaseBrushProperty =
            DependencyProperty.Register(nameof(BaseBrush), typeof(Brush), typeof(BoardGridView),
                new PropertyMetadata(Frozen(0xB1, 0xC4, 0xD3)));

        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(BoardGridView),
                new PropertyMetadata(Frozen(0x7F, 0x93, 0xA3)));

        public static readonly DependencyProperty FrameBrushProperty =
            DependencyProperty.Register(nameof(FrameBrush), typeof(Brush), typeof(BoardGridView),
                new PropertyMetadata(Frozen(0x21, 0x3A, 0x58)));

        public static readonly DependencyProperty IsInteractiveProperty =
            DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(BoardGridView),
                new PropertyMetadata(false));

        public BoardGridView()
        {
            InitializeComponent();
        }

        public Brush BaseBrush
        {
            get => (Brush)GetValue(BaseBrushProperty);
            set => SetValue(BaseBrushProperty, value);
        }

        public Brush LineBrush
        {
            get => (Brush)GetValue(LineBrushProperty);
            set => SetValue(LineBrushProperty, value);
        }

        public Brush FrameBrush
        {
            get => (Brush)GetValue(FrameBrushProperty);
            set => SetValue(FrameBrushProperty, value);
        }

        /// <summary>True: các ô bấm được (bàn cờ địch). False: chỉ để xem (bàn cờ của mình, lưới đặt tàu).</summary>
        public bool IsInteractive
        {
            get => (bool)GetValue(IsInteractiveProperty);
            set => SetValue(IsInteractiveProperty, value);
        }

        private static SolidColorBrush Frozen(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}
