using BattleShip.Application;

namespace BattleShip.Wpf.Services
{
    
    public static class BotFactory
    {
        public static IBotStrategy Create(string difficulty) => difficulty switch
        {
            "Easy" => new RandomBot(),
            "Hard" => new ProbabilityBot(),
            _ => new HuntTargetBot(),
        };
    }
}
