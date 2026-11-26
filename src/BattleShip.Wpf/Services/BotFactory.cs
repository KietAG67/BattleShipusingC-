using BattleShip.Application;

namespace BattleShip.Wpf.Services
{
    /// <summary>Chọn bot theo độ khó người chơi đã chọn. Thuật toán bot nằm trong Application (Kiệt).</summary>
    public static class BotFactory
    {
        public static IBotStrategy Create(string difficulty) => difficulty switch
        {
            "Easy" => new EasyBot(),
            "Hard" => new HardBot(),
            _ => new MediumBot(),
        };
    }
}
