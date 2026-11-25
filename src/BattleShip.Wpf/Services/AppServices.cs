using BattleShip.Application.Interfaces;
using BattleShip.Infrastructure;

namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// Nơi duy nhất tạo các service dùng chung. Đăng nhập, lưu trận và bảng xếp hạng dùng SQL Server;
    /// chuỗi kết nối đọc từ appsettings.json (xem DatabaseConfig trong Infrastructure).
    /// </summary>
    public static class AppServices
    {
        private static readonly GameRepository GameRepo = new();

        public static SqlAuthService SqlAuth { get; } = new(new UserRepository());

        public static IAuthService Auth { get; set; } = SqlAuth;

        public static IGameRepository Games { get; set; } = GameRepo;

        public static IShotRepository Shots { get; set; } = new ShotRepository();

        public static ILeaderboardRepository Leaderboard { get; set; } = GameRepo;

        /// <summary>Id người chơi hiện tại; null nếu dùng service giả hoặc chưa đăng nhập.</summary>
        public static int? CurrentUserId => SqlAuth.CurrentUserId;
    }
}
