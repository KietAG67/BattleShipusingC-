using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class PlayerScore
    {
        public int Rank { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Wins { get; set; }
    }

    /// <summary>Bảng xếp hạng lấy từ SQL Server theo số trận thắng.</summary>
    public class LeaderboardViewModel : ViewModelBase
    {
        private const int TopCount = 10;

        private string _statusMessage = string.Empty;

        public ObservableCollection<PlayerScore> TopPlayers { get; } = new();

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand BackToMenuCommand { get; }

        public LeaderboardViewModel(NavigationStore navigationStore)
        {
            BackToMenuCommand = new RelayCommand(_ => navigationStore.CurrentViewModel = new MainMenuViewModel(navigationStore));
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var entries = await AppServices.Leaderboard.GetTopPlayersAsync(TopCount);
                foreach (var entry in entries)
                {
                    TopPlayers.Add(new PlayerScore { Rank = entry.Rank, Username = entry.Username, Wins = entry.Wins });
                }

                StatusMessage = TopPlayers.Count == 0 ? "No games played yet." : string.Empty;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                StatusMessage = "Cannot load the leaderboard. Check that SQL Server is running.";
            }
        }
    }
}
