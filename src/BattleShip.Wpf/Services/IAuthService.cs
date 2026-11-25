namespace BattleShip.Wpf.Services
{
    public sealed record AuthResult(bool Success, string Message);

    /// <summary>
    /// Giao diện đăng nhập/đăng ký mà LoginViewModel cần.
    /// Phần SQL Server là của Sỹ; trong lúc chờ, WPF dùng FakeAuthService (lưu trong RAM).
    /// </summary>
    public interface IAuthService
    {
        AuthResult Login(string username, string password);

        AuthResult Register(string username, string password);
    }
}
