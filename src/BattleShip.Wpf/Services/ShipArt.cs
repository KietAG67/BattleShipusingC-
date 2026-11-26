using System.Windows.Media;
using BattleShip.Wpf.Helpers;

namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// Ghép tên tàu trong Domain (Fleet.Default có 5 tàu) với 4 hình tàu đang có.
    /// Submarine (3 ô) dùng chung hình Cruiser vì cùng dài 3 ô.
    /// </summary>
    public static class ShipArt
    {
        public static string KeyFor(string shipName) => shipName switch
        {
            "Carrier" => "carrier",
            "Battleship" => "battleship",
            "Cruiser" => "cruiser",
            "Submarine" => "cruiser",
            "Destroyer" => "destroyer",
            _ => "destroyer",
        };

        public static ImageSource Get(string shipName, bool vertical)
            => AssetImages.Get($"Ships/{KeyFor(shipName)}_{(vertical ? "v" : "h")}.png");
    }
}
