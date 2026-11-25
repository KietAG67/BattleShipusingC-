using System.Collections.ObjectModel;
using System.Windows.Input;
using BattleShip.Wpf.Commands;

namespace BattleShip.Wpf.ViewModels
{
    public class PlayerScore
    {
        public int Rank { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Wins { get; set; }
    }

    public class LeaderboardViewModel : ViewModelBase
    {
        public ObservableCollection<PlayerScore> TopPlayers { get; }
        public ICommand BackToMenuCommand { get; }

        public LeaderboardViewModel(NavigationStore navigationStore)
        {
            TopPlayers = new ObservableCollection<PlayerScore>
            {
                new PlayerScore { Rank = 1, Username = "DevNam", Wins = 42 },
                new PlayerScore { Rank = 2, Username = "TeamKiet", Wins = 35 },
                new PlayerScore { Rank = 3, Username = "SyMaster", Wins = 28 }
            };

            BackToMenuCommand = new RelayCommand(_ => navigationStore.CurrentViewModel = new MainMenuViewModel(navigationStore));
        }
    }
}