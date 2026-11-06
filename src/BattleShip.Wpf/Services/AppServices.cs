namespace BattleShip.Wpf.Services
{
    
    public static class AppServices
    {
        public static IAuthService Auth { get; set; } = new FakeAuthService();
    }
}
