using System.Collections.ObjectModel;
using System.Windows.Input;
using BattleShip.Application;
using BattleShip.Domain;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class FleetPlacementViewModel : ViewModelBase
    {
        private sealed record PlacedShip(int FleetIndex, Cell Start, Orientation Orientation);

        private readonly NavigationStore _navigationStore;
        private readonly List<PlacedShip> _placed = new();
        private Board _scratch = new();
        private int _selectedIndex;
        private bool _isVertical;
        private Cell? _hover;
        private ShipSpriteViewModel? _previewSprite;
        private string _statusText = string.Empty;

        public FleetPlacementViewModel(NavigationStore navigationStore, string difficulty = "Medium")
        {
            _navigationStore = navigationStore;
            Difficulty = difficulty;

            PlacementBoard = new BoardViewModel();
            Cards = new ObservableCollection<ShipCardViewModel>
            {
                new ShipCardViewModel("Aircraft Carrier", 0),
                new ShipCardViewModel("Battleship", 1),
                new ShipCardViewModel("Cruiser", 2),
                new ShipCardViewModel("Destroyer", 3),
            };

            SelectCardCommand = new RelayCommand(p => SelectCard(p as ShipCardViewModel));
            RotateCommand = new RelayCommand(_ => Rotate());
            ClearCommand = new RelayCommand(_ => Clear());
            ConfirmCommand = new RelayCommand(_ => Confirm(), _ => CanConfirm);
            BackCommand = new RelayCommand(_ => _navigationStore.CurrentViewModel = new MainMenuViewModel(_navigationStore));

            _selectedIndex = 0;
            RefreshAll();
        }

        public string Difficulty { get; }

        public string DifficultyText => $"Difficulty: {Difficulty}";

        public BoardViewModel PlacementBoard { get; }

        public ObservableCollection<ShipCardViewModel> Cards { get; }

        public bool IsVertical => _isVertical;

        public string StatusText
        {
            get => _statusText;
            private set => SetField(ref _statusText, value);
        }

        public bool CanConfirm => _placed.Count == Fleet.Default.Count;

        public ICommand SelectCardCommand { get; }

        public ICommand RotateCommand { get; }

        public ICommand ClearCommand { get; }

        public ICommand ConfirmCommand { get; }

        public ICommand BackCommand { get; }

        private Orientation CurrentOrientation => _isVertical ? Orientation.Vertical : Orientation.Horizontal;

        // ----- được View gọi khi chuột di chuyển trên lưới -----

        public void Hover(int row, int column)
        {
            _hover = new Cell(row, column);
            UpdatePreview();
        }

        public void ClearHover()
        {
            _hover = null;
            UpdatePreview();
        }

        /// <summary>Chuột trái: đặt tàu đang chọn tại ô đang rê chuột.</summary>
        public void PlaceAtHover()
        {
            if (_hover is not { } hover || _selectedIndex < 0)
            {
                return;
            }

            var (name, length) = Fleet.Default[_selectedIndex];
            var start = ClampStart(hover, length);

            // Việc tàu có hợp lệ hay không do Domain (Board.CanPlaceShip) quyết định.
            if (!_scratch.CanPlaceShip(length, start, CurrentOrientation))
            {
                StatusText = $"Cannot place {name} here: it overlaps another ship.";
                return;
            }

            _scratch.PlaceShip(name, length, start, CurrentOrientation);
            _placed.Add(new PlacedShip(_selectedIndex, start, CurrentOrientation));
            _selectedIndex = NextUnplacedIndex();
            RefreshAll();
        }

        /// <summary>Chuột phải lên một con tàu đã đặt: nhấc tàu đó lên để đặt lại.</summary>
        public void PickUpAt(int row, int column)
        {
            var target = new Cell(row, column);
            var hit = _placed.FirstOrDefault(p =>
                Board.GetCells(Fleet.Default[p.FleetIndex].Length, p.Start, p.Orientation).Contains(target));

            if (hit is null)
            {
                return;
            }

            _isVertical = hit.Orientation == Orientation.Vertical;
            RemovePlaced(hit.FleetIndex);
            _selectedIndex = hit.FleetIndex;
            RefreshAll();
        }

        // ----- lệnh -----

        private void SelectCard(ShipCardViewModel? card)
        {
            if (card is null)
            {
                return;
            }

            var free = card.FleetIndices.Where(i => !IsPlaced(i)).ToList();
            if (free.Count > 0)
            {
                _selectedIndex = free[0];
            }
            else
            {
                // Thẻ đã đặt hết: bấm vào để nhấc con cuối cùng lên đặt lại.
                var index = card.FleetIndices[^1];
                RemovePlaced(index);
                _selectedIndex = index;
            }

            RefreshAll();
        }

        private void Rotate()
        {
            _isVertical = !_isVertical;
            OnPropertyChanged(nameof(IsVertical));
            UpdatePreview();
            UpdateStatus(null);
        }

        private void Clear()
        {
            _placed.Clear();
            RebuildScratch();
            _selectedIndex = 0;
            RefreshAll();
        }

        private void Confirm()
        {
            if (!CanConfirm)
            {
                return;
            }

            var session = new GameSession();
            foreach (var placed in _placed)
            {
                var (name, length) = Fleet.Default[placed.FleetIndex];
                session.PlayerBoard.PlaceShip(name, length, placed.Start, placed.Orientation);
            }

            RandomFleetPlacer.PlaceFleet(session.BotBoard);
            session.Start();

            _navigationStore.CurrentViewModel = new BattleViewModel(_navigationStore, session, Difficulty);
        }

        // ----- nội bộ -----

        private bool IsPlaced(int fleetIndex) => _placed.Any(p => p.FleetIndex == fleetIndex);

        private int NextUnplacedIndex()
        {
            for (var i = 0; i < Fleet.Default.Count; i++)
            {
                if (!IsPlaced(i))
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemovePlaced(int fleetIndex)
        {
            _placed.RemoveAll(p => p.FleetIndex == fleetIndex);
            RebuildScratch();
        }

        private void RebuildScratch()
        {
            _scratch = new Board();
            foreach (var placed in _placed)
            {
                var (name, length) = Fleet.Default[placed.FleetIndex];
                _scratch.PlaceShip(name, length, placed.Start, placed.Orientation);
            }
        }

        /// <summary>Dịch điểm bắt đầu vào trong để tàu không bị tràn ra ngoài lưới khi rê chuột sát mép.</summary>
        private Cell ClampStart(Cell hover, int length)
        {
            return _isVertical
                ? new Cell(Math.Min(hover.Row, Board.Size - length), hover.Column)
                : new Cell(hover.Row, Math.Min(hover.Column, Board.Size - length));
        }

        private void RefreshAll()
        {
            PlacementBoard.Ships.Clear();
            _previewSprite = null;

            foreach (var placed in _placed)
            {
                var (name, length) = Fleet.Default[placed.FleetIndex];
                PlacementBoard.Ships.Add(BoardViewModel.CreateSprite(
                    name, placed.Start.Row, placed.Start.Column, length, placed.Orientation == Orientation.Vertical));
            }

            foreach (var card in Cards)
            {
                card.PlacedCount = card.FleetIndices.Count(IsPlaced);
                card.IsSelected = card.FleetIndices.Contains(_selectedIndex);
            }

            OnPropertyChanged(nameof(CanConfirm));
            RelayCommand.Refresh();
            UpdatePreview();
            UpdateStatus(null);
        }

        private void UpdatePreview()
        {
            PlacementBoard.ClearPreview();
            if (_previewSprite is not null)
            {
                PlacementBoard.Ships.Remove(_previewSprite);
                _previewSprite = null;
            }

            if (_hover is not { } hover || _selectedIndex < 0)
            {
                return;
            }

            var (name, length) = Fleet.Default[_selectedIndex];
            var start = ClampStart(hover, length);
            var valid = _scratch.CanPlaceShip(length, start, CurrentOrientation);

            PlacementBoard.SetPreview(Board.GetCells(length, start, CurrentOrientation), valid);
            _previewSprite = BoardViewModel.CreateSprite(name, start.Row, start.Column, length, _isVertical, valid ? 0.75 : 0.45);
            PlacementBoard.Ships.Add(_previewSprite);

            if (!valid)
            {
                UpdateStatus($"Cannot place {name} here: it overlaps another ship.");
            }
            else
            {
                UpdateStatus(null);
            }
        }

        private void UpdateStatus(string? warning)
        {
            if (warning is not null)
            {
                StatusText = warning;
                return;
            }

            if (_selectedIndex < 0)
            {
                StatusText = "Fleet complete - press CONFIRM to start the battle.";
                return;
            }

            var (name, length) = Fleet.Default[_selectedIndex];
            StatusText = $"Placing: {name} ({length} cells) • {(_isVertical ? "Vertical" : "Horizontal")} • R: rotate";
        }
    }
}
