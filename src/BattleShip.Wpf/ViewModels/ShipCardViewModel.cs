namespace BattleShip.Wpf.ViewModels
{
    /// <summary>Một thẻ trong bảng "Available Ships" của ảnh nền (mỗi thẻ ứng với một tàu trong Fleet.Default).</summary>
    public sealed class ShipCardViewModel : ViewModelBase
    {
        private int _placedCount;
        private bool _isSelected;

        public ShipCardViewModel(string title, params int[] fleetIndices)
        {
            Title = title;
            FleetIndices = fleetIndices;
        }

        public string Title { get; }

        /// <summary>Chỉ số các tàu trong Fleet.Default mà thẻ này đại diện.</summary>
        public int[] FleetIndices { get; }

        public int Total => FleetIndices.Length;

        public int PlacedCount
        {
            get => _placedCount;
            set
            {
                if (SetField(ref _placedCount, value))
                {
                    OnPropertyChanged(nameof(IsDone));
                    OnPropertyChanged(nameof(BadgeText));
                    OnPropertyChanged(nameof(HasBadge));
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public bool IsDone => _placedCount >= Total;

        public string BadgeText => IsDone ? "✓" : (Total > 1 ? $"x{Total - _placedCount}" : string.Empty);

        public bool HasBadge => BadgeText.Length > 0;
    }
}
