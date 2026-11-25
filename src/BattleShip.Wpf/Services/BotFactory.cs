using BattleShip.Application;

namespace BattleShip.Wpf.Services
{
    
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
