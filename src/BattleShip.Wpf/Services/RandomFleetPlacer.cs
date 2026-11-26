using BattleShip.Domain;

namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// TẠM THỜI: rải hạm đội cho bot một cách ngẫu nhiên để màn hình Battle chạy được.
    /// Luật hợp lệ (vượt biên/chồng tàu) vẫn do Board.CanPlaceShip quyết định, ở đây chỉ chọn vị trí thử.
    /// Khi Kiệt/Sỹ bàn giao hàm đặt tàu cho bot trong Application thì thay chỗ gọi trong FleetPlacementViewModel.
    /// </summary>
    public static class RandomFleetPlacer
    {
        private const int MaxAttemptsPerShip = 2000;

        public static void PlaceFleet(Board board, Random? random = null)
        {
            var rng = random ?? Random.Shared;

            foreach (var (name, length) in Fleet.Default)
            {
                var placed = false;
                for (var attempt = 0; attempt < MaxAttemptsPerShip && !placed; attempt++)
                {
                    var orientation = rng.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical;
                    var start = new Cell(rng.Next(Board.Size), rng.Next(Board.Size));

                    if (board.CanPlaceShip(length, start, orientation))
                    {
                        board.PlaceShip(name, length, start, orientation);
                        placed = true;
                    }
                }

                if (!placed)
                {
                    throw new InvalidOperationException($"Could not place ship {name} for the bot.");
                }
            }
        }
    }
}
