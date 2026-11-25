using System.Windows.Input;
using System.Windows.Threading;
using BattleShip.Application;
using BattleShip.Domain;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    /// <summary>
    /// Màn hình chiến đấu. ViewModel chỉ điều phối giao diện: mọi luật bắn, đổi lượt, thắng/thua đều do
    /// GameSession (Application) và Board (Domain) quyết định; bot chọn ô bằng IBotStrategy.
    /// </summary>
    public class BattleViewModel : ViewModelBase
    {
        private const int TurnSeconds = 30;
        private const int BotThinkMilliseconds = 800;
        private const int EndDelayMilliseconds = 1800;

        private readonly NavigationStore _navigationStore;
        private readonly GameSession _session;
        private readonly IBotStrategy _bot;
        private readonly DispatcherTimer _timer;
        private readonly HashSet<Ship> _revealedEnemyShips = new();

        private CellViewModel? _selectedCell;
        private int _playerShots;
        private int _remaining;
        private bool _busy;
        private bool _closed;
        private string _turnLabel = "YOUR TURN";
        private string _timerText = "00:30";
        private string _statusMessage = string.Empty;

        public BattleViewModel(NavigationStore navigationStore, GameSession session, string difficulty = "Medium")
        {
            _navigationStore = navigationStore;
            _session = session;
            Difficulty = difficulty;
            _bot = BotFactory.Create(difficulty);

            PlayerBoard = new BoardViewModel();
            EnemyBoard = new BoardViewModel(OnEnemyCellClicked);

            foreach (var ship in _session.PlayerBoard.Ships)
            {
                PlayerBoard.AddShipSprite(ship);
            }

            PlayerBoard.Refresh(_session.PlayerBoard, revealShips: true);
            EnemyBoard.Refresh(_session.BotBoard, revealShips: false);

            FireCommand = new RelayCommand(async _ => await FireSafeAsync(), _ => CanFire);
            SurrenderCommand = new RelayCommand(_ => Surrender());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTick;

            BeginPlayerTurn("Your turn: pick a cell on Enemy Waters, then press Fire.");
        }

        public string Difficulty { get; }

        public BoardViewModel PlayerBoard { get; }

        public BoardViewModel EnemyBoard { get; }

        public ICommand FireCommand { get; }

        public ICommand SurrenderCommand { get; }

        /// <summary>
        /// View gắn hiệu ứng đạn bay vào đây: (bắnTừNgườiChơi, hàng, cột). Nếu để null thì bỏ qua hiệu ứng.
        /// </summary>
        public Func<bool, int, int, Task>? PlayMissileAsync { get; set; }

        public string TurnLabel
        {
            get => _turnLabel;
            private set => SetField(ref _turnLabel, value);
        }

        public string TimerText
        {
            get => _timerText;
            private set => SetField(ref _timerText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetField(ref _statusMessage, value);
        }

        /// <summary>True khi chưa thể bắn (không phải lượt, đang animation, hoặc chưa chọn ô) để View làm mờ nút Fire.</summary>
        public bool IsFireDimmed => !CanFire;

        private bool CanFire =>
            !_busy
            && !_closed
            && _session.Status == GameStatus.InProgress
            && _session.IsPlayerTurn
            && _selectedCell is not null;

        // ---------- người chơi ----------

        private void OnEnemyCellClicked(CellViewModel cell)
        {
            if (_busy || _closed || !_session.IsPlayerTurn || _session.Status != GameStatus.InProgress)
            {
                return;
            }

            if (!cell.IsClickable)
            {
                return; // ô đã bắn
            }

            if (ReferenceEquals(_selectedCell, cell))
            {
                // Bấm lại đúng ô đang ngắm = bắn luôn.
                if (FireCommand.CanExecute(null))
                {
                    FireCommand.Execute(null);
                }

                return;
            }

            ClearSelection();
            _selectedCell = cell;
            cell.IsSelected = true;
            StatusMessage = $"Target {cell.Label}: press Fire (or click it again).";
            RefreshFireState();
        }

        private async Task FireSafeAsync()
        {
            try
            {
                await FireAsync();
            }
            catch (Exception)
            {
                _busy = false;
                if (!_closed)
                {
                    BeginPlayerTurn("Something went wrong. Please try again.");
                }
            }
        }

        private async Task FireAsync()
        {
            if (!CanFire)
            {
                return;
            }

            var target = _selectedCell!;
            var cell = target.Cell;
            var label = target.Label;

            SetBusy(true);
            _timer.Stop();

            try
            {
                var result = _session.PlayerShoot(cell);
                _playerShots++;

                await LaunchMissileAsync(fromPlayer: true, cell);
                if (_closed)
                {
                    return;
                }

                ClearSelection();
                EnemyBoard.Refresh(_session.BotBoard, revealShips: false);
                RevealSunkShips();

                switch (result)
                {
                    case ShotResult.GameOver:
                        await EndAsync(playerWon: true);
                        break;
                    case ShotResult.Miss:
                        StatusMessage = $"{label}: miss. Enemy turn.";
                        await BotTurnAsync();
                        break;
                    case ShotResult.Sunk:
                        BeginPlayerTurn($"{label}: hit! You sank an enemy ship. Fire again!");
                        break;
                    case ShotResult.Hit:
                        BeginPlayerTurn($"{label}: hit! Fire again!");
                        break;
                    default:
                        BeginPlayerTurn($"{label} was already fired at. Pick another cell.");
                        break;
                }
            }
            finally
            {
                _busy = false;
                if (!_closed)
                {
                    RefreshFireState();
                }
            }
        }

        // ---------- bot ----------

        private async Task BotTurnAsync()
        {
            TurnLabel = "ENEMY TURN";
            TimerText = "--:--";

            while (!_closed && _session.Status == GameStatus.InProgress && !_session.IsPlayerTurn)
            {
                await Task.Delay(BotThinkMilliseconds);
                if (_closed)
                {
                    return;
                }

                var cell = _bot.ChooseTarget(_session.PlayerBoard);
                var result = _session.BotShoot(cell);
                _bot.ProcessResult(cell, result);

                await LaunchMissileAsync(fromPlayer: false, cell);
                if (_closed)
                {
                    return;
                }

                PlayerBoard.Refresh(_session.PlayerBoard, revealShips: true);

                if (result == ShotResult.GameOver)
                {
                    await EndAsync(playerWon: false);
                    return;
                }

                var label = Label(cell);
                StatusMessage = result switch
                {
                    ShotResult.Sunk => $"Enemy fired at {label}: your ship was sunk!",
                    ShotResult.Hit => $"Enemy hit {label}!",
                    ShotResult.Miss => $"Enemy missed at {label}.",
                    _ => $"Enemy fired at {label}.",
                };
            }

            if (!_closed && _session.Status == GameStatus.InProgress)
            {
                BeginPlayerTurn("Your turn! Pick a cell, then press Fire.");
            }
        }

        // ---------- lượt & đồng hồ ----------

        private void BeginPlayerTurn(string message)
        {
            _remaining = TurnSeconds;
            UpdateTimerText();
            TurnLabel = "YOUR TURN";
            StatusMessage = message;

            if (!_closed)
            {
                _timer.Start();
            }

            RefreshFireState();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (_busy || _closed)
            {
                return;
            }

            _remaining--;
            UpdateTimerText();

            if (_remaining <= 0)
            {
                _timer.Stop();
                _ = OnTimeoutAsync();
            }
        }

        private async Task OnTimeoutAsync()
        {
            SetBusy(true);
            try
            {
                ClearSelection();
#if HAS_TURN_TIMEOUT
                // Cần Sỹ thêm GameSession.TimeoutTurn(): coi như bắn trượt và chuyển lượt cho bot.
                _session.TimeoutTurn();
                StatusMessage = "Time is up! You lose your turn.";
                await BotTurnAsync();
#else
                // GameSession hiện chưa có hàm đổi lượt khi hết giờ nên UI chỉ báo và cho đếm lại 30 giây.
                BeginPlayerTurn("Time is up!");
                await Task.CompletedTask;
#endif
            }
            catch (Exception)
            {
                StatusMessage = "Something went wrong. Please try again.";
            }
            finally
            {
                _busy = false;
                if (!_closed)
                {
                    RefreshFireState();
                }
            }
        }

        private void UpdateTimerText()
            => TimerText = $"{_remaining / 60:00}:{_remaining % 60:00}";

        // ---------- kết thúc ----------

        private async Task EndAsync(bool playerWon)
        {
            _timer.Stop();
            TurnLabel = playerWon ? "VICTORY" : "DEFEAT";
            TimerText = "--:--";
            StatusMessage = playerWon
                ? "You destroyed the entire enemy fleet!"
                : "Your entire fleet was sunk!";

            await Task.Delay(EndDelayMilliseconds);
            if (_closed)
            {
                return;
            }

            Close();
            _navigationStore.CurrentViewModel = new ResultViewModel(_navigationStore, playerWon, _playerShots, Difficulty);
        }

        /// <summary>Đầu hàng (phím Esc trong BattleView): tính là thua.</summary>
        public void Surrender()
        {
            if (_closed)
            {
                return;
            }

            Close();
            _navigationStore.CurrentViewModel = new ResultViewModel(_navigationStore, false, _playerShots, Difficulty);
        }

        private void Close()
        {
            _closed = true;
            _timer.Stop();
            _timer.Tick -= OnTick;
        }

        // ---------- tiện ích ----------

        private void RevealSunkShips()
        {
            // Chỉ lộ hình những tàu địch ĐÃ CHÌM; tàu chưa chìm vẫn được giấu.
            foreach (var ship in _session.BotBoard.Ships)
            {
                if (ship.IsSunk && _revealedEnemyShips.Add(ship))
                {
                    EnemyBoard.AddShipSprite(ship);
                }
            }
        }

        private void ClearSelection()
        {
            if (_selectedCell is not null)
            {
                _selectedCell.IsSelected = false;
                _selectedCell = null;
            }
        }

        private void SetBusy(bool value)
        {
            _busy = value;
            RefreshFireState();
        }

        private void RefreshFireState()
        {
            OnPropertyChanged(nameof(IsFireDimmed));
            RelayCommand.Refresh();
        }

        private Task LaunchMissileAsync(bool fromPlayer, Cell cell)
            => PlayMissileAsync is null ? Task.CompletedTask : PlayMissileAsync(fromPlayer, cell.Row, cell.Column);

        private static string Label(Cell cell) => $"{(char)('A' + cell.Row)}{cell.Column + 1}";
    }
}
