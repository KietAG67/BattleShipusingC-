using System.Windows.Input;
using BattleShip.Wpf.Commands;

namespace BattleShip.Wpf.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private bool _isDifficultyOpen;

        public MainMenuViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            // START GAME -> mở bảng chọn độ khó -> Fleet Placement (đúng scene flow trong docs/architecture.md).
            StartGameCommand = new RelayCommand(_ => IsDifficultyOpen = true);
            SelectDifficultyCommand = new RelayCommand(SelectDifficulty);
            CancelDifficultyCommand = new RelayCommand(_ => IsDifficultyOpen = false);
            LeaderboardCommand = new RelayCommand(_ => _navigationStore.CurrentViewModel = new LeaderboardViewModel(_navigationStore));
            ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
        }

        public bool IsDifficultyOpen
        {
            get => _isDifficultyOpen;
            set => SetField(ref _isDifficultyOpen, value);
        }

        public ICommand StartGameCommand { get; }

        public ICommand SelectDifficultyCommand { get; }

        public ICommand CancelDifficultyCommand { get; }

        public ICommand LeaderboardCommand { get; }

        public ICommand ExitCommand { get; }

        private void SelectDifficulty(object? parameter)
        {
            var difficulty = parameter as string ?? "Medium";
            IsDifficultyOpen = false;
            _navigationStore.CurrentViewModel = new FleetPlacementViewModel(_navigationStore, difficulty);
        }
    }
}
