using System.Windows;
using BattleShip.Wpf.ViewModels;

namespace BattleShip.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var navigationStore = new NavigationStore();
            navigationStore.CurrentViewModel = new LoginViewModel(navigationStore);
            DataContext = new MainViewModel(navigationStore);
        }
    }
}
