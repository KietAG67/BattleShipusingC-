using System.Windows.Media;
using BattleShip.Wpf.Helpers;

namespace BattleShip.Wpf.Services
{
    
    public static class ShipArt
    {
        public static string KeyFor(string shipName) => shipName switch
        {
            "Carrier" => "carrier",
            "Battleship" => "battleship",
            "Cruiser" => "cruiser",
            "Destroyer" => "destroyer",
            _ => "destroyer",
        };

        public static ImageSource Get(string shipName, bool vertical)
            => AssetImages.Get($"Ships/{KeyFor(shipName)}_{(vertical ? "v" : "h")}.png");
    }
}
