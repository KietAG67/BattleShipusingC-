namespace BattleShip.Wpf.Services
{
    public sealed record AuthResult(bool Success, string Message);

    
    public interface IAuthService
    {
        AuthResult Login(string username, string password);

        AuthResult Register(string username, string password);
    }
}
