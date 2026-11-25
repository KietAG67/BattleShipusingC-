using BattleShip.Application.Interfaces;

namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// Đăng nhập/đăng ký thật bằng SQL Server (UserRepository, mật khẩu được hash).
    /// Lỗi kết nối được báo lại cho người dùng thay vì làm app crash.
    /// </summary>
    public sealed class SqlAuthService : IAuthService
    {
        private readonly IUserRepository _users;

        public SqlAuthService(IUserRepository users)
        {
            _users = users;
        }

        /// <summary>Id người dùng đang đăng nhập; null khi chưa đăng nhập.</summary>
        public int? CurrentUserId { get; private set; }

        public AuthResult Login(string username, string password)
        {
            var result = Run(() => _users.LoginAsync(username, password));
            CurrentUserId = result.Success ? result.User?.Id : null;
            return new AuthResult(result.Success, result.Message);
        }

        public AuthResult Register(string username, string password)
        {
            var result = Run(() => _users.RegisterAsync(username, password));
            return new AuthResult(result.Success, result.Message);
        }

        private static BattleShip.Application.Interfaces.AuthResult Run(
            Func<Task<BattleShip.Application.Interfaces.AuthResult>> action)
        {
            try
            {
                return Task.Run(action).GetAwaiter().GetResult();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return new BattleShip.Application.Interfaces.AuthResult(
                    false, "Cannot connect to the database. Check that SQL Server is running.");
            }
        }
    }
}
