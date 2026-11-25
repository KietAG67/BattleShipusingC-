namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// Nơi duy nhất để thay các service "giả" bằng service thật khi các thành viên khác bàn giao.
    /// Ví dụ khi Sỹ xong UserRepository: AppServices.Auth = new SqlAuthService(...);
    /// </summary>
    public static class AppServices
    {
        public static IAuthService Auth { get; set; } = new FakeAuthService();
    }
}
