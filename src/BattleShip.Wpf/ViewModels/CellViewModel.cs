using System.Windows.Input;
using BattleShip.Domain;
using BattleShip.Wpf.Commands;

namespace BattleShip.Wpf.ViewModels
{
    public enum PreviewKind
    {
        None,
        Valid,
        Invalid,
    }

    /// <summary>Một ô 10x10 trên giao diện. Chỉ chứa trạng thái hiển thị, không có luật game.</summary>
    public sealed class CellViewModel : ViewModelBase
    {
        private CellState _state = CellState.Unknown;
        private bool _isSelected;
        private PreviewKind _preview = PreviewKind.None;

        public CellViewModel(int row, int column, Action<CellViewModel>? onClick)
        {
            Row = row;
            Column = column;
            ClickCommand = new RelayCommand(_ => onClick?.Invoke(this));
        }

        public int Row { get; }

        public int Column { get; }

        public Cell Cell => new(Row, Column);

        /// <summary>Ví dụ "A1", "J10" (hàng A-J, cột 1-10) giống nhãn trên ảnh nền.</summary>
        public string Label => $"{(char)('A' + Row)}{Column + 1}";

        public ICommand ClickCommand { get; }

        public CellState State
        {
            get => _state;
            set
            {
                if (SetField(ref _state, value))
                {
                    OnPropertyChanged(nameof(IsHit));
                    OnPropertyChanged(nameof(IsMiss));
                    OnPropertyChanged(nameof(IsSunk));
                    OnPropertyChanged(nameof(IsClickable));
                }
            }
        }

        /// <summary>Hit và Sunk dùng chung một hiệu ứng nên animation không bị cắt khi tàu vừa chìm.</summary>
        public bool IsHit => _state is CellState.Hit or CellState.Sunk;

        public bool IsMiss => _state == CellState.Miss;

        public bool IsSunk => _state == CellState.Sunk;

        /// <summary>Ô đã bắn thì bị vô hiệu hóa.</summary>
        public bool IsClickable => _state is CellState.Unknown or CellState.Ship;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public PreviewKind Preview
        {
            get => _preview;
            set
            {
                if (SetField(ref _preview, value))
                {
                    OnPropertyChanged(nameof(IsPreviewValid));
                    OnPropertyChanged(nameof(IsPreviewInvalid));
                }
            }
        }

        public bool IsPreviewValid => _preview == PreviewKind.Valid;

        public bool IsPreviewInvalid => _preview == PreviewKind.Invalid;
    }
}
