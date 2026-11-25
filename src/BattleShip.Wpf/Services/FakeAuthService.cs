using System.Security.Cryptography;
using System.Text;

namespace BattleShip.Wpf.Services
{
    
    public sealed class FakeAuthService : IAuthService
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 50;
        private const int MinPasswordLength = 6;

        private readonly Dictionary<string, string> _users = new(StringComparer.OrdinalIgnoreCase);

        public FakeAuthService()
        {
            _users["demo"] = Hash("demo123");
        }

        public AuthResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return new AuthResult(false, "Please enter your username and password.");
            }

            return _users.TryGetValue(username.Trim(), out var hash) && hash == Hash(password)
                ? new AuthResult(true, "Login successful.")
                : new AuthResult(false, "Incorrect username or password.");
        }

        public AuthResult Register(string username, string password)
        {
            var name = username?.Trim() ?? string.Empty;

            if (name.Length < MinUsernameLength || name.Length > MaxUsernameLength)
            {
                return new AuthResult(false, $"Username must be {MinUsernameLength}-{MaxUsernameLength} characters.");
            }

            if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            {
                return new AuthResult(false, $"Password must be at least {MinPasswordLength} characters.");
            }

            if (_users.ContainsKey(name))
            {
                return new AuthResult(false, "This username is already taken.");
            }

            _users[name] = Hash(password);
            return new AuthResult(true, "Account created! Press LOGIN to continue.");
        }

        private static string Hash(string password)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
}
