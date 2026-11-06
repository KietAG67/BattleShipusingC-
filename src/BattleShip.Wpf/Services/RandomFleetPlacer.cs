using BattleShip.Domain;

namespace BattleShip.Wpf.Services
{
    
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
