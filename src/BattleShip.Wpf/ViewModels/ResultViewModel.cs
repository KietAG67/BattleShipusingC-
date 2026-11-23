using System.Windows.Input;
using BattleShip.Wpf.Commands;

namespace BattleShip.Wpf.ViewModels
{
    public class ResultViewModel : ViewModelBase
    {
        public ResultViewModel(NavigationStore navigationStore, bool isWin, int totalShots, string difficulty = "Medium")
        {
            IsWin = isWin;
            ResultTitle = isWin ? "VICTORY!" : "DEFEAT!";
            ResultColor = isWin ? "#A6E3A1" : "#F38BA8";
            TotalShots = totalShots;
            ShotsText = $"Shots fired: {totalShots}";

            PlayAgainCommand = new RelayCommand(_ => navigationStore.CurrentViewModel = new FleetPlacementViewModel(navigationStore, difficulty));
            MainMenuCommand = new RelayCommand(_ => navigationStore.CurrentViewModel = new MainMenuViewModel(navigationStore));
        }

        public bool IsWin { get; }

        public bool IsDefeat => !IsWin;

        public string ResultTitle { get; }

        public string ResultColor { get; }

        public int TotalShots { get; }

        public string ShotsText { get; }

        public ICommand PlayAgainCommand { get; }

        /// <summary>Nút QUIT GAME trên ảnh: thoát trận về Main Menu.</summary>
        public ICommand MainMenuCommand { get; }
    }
}
