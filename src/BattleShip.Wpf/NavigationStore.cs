namespace BattleShip.Wpf.ViewModels
{
    /// <summary>Giữ ViewModel đang hiển thị. MainWindow chỉ việc hiển thị CurrentViewModel.</summary>
    public class NavigationStore
    {
        private ViewModelBase? _currentViewModel;

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                CurrentViewModelChanged?.Invoke();
            }
        }

        public event Action? CurrentViewModelChanged;
    }
}
