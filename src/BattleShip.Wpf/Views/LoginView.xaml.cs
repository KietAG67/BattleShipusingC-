using System.Windows.Controls;

namespace BattleShip.Wpf.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += (_, _) => UsernameBox.Focus();
        }
    }
}
