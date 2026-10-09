using System.Windows; using System.Windows.Controls; using BattleShip.Domain;
namespace BattleShip.Wpf; public partial class MainWindow:Window { public MainWindow(){InitializeComponent();BuildGrid(PlayerGrid);BuildGrid(EnemyGrid);} private static void BuildGrid(Panel grid){for(var i=0;i<100;i++)grid.Children.Add(new Button{Margin=new Thickness(1),Tag=new Cell(i/10,i%10),Content="·"});} }
