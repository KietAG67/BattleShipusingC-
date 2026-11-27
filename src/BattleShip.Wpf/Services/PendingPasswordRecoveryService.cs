namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// Chỗ cắm tạm khi chưa có backend quên mật khẩu. Không giả vờ đã gửi email:
    /// trả về thông báo rõ ràng để người chơi biết tính năng chưa sẵn sàng.
    /// </summary>
    public sealed class PendingPasswordRecoveryService : IPasswordRecoveryService
    {
        public const string NotReadyMessage = "Password recovery is not connected to the database yet.";

        public AuthResult SendVerificationCode(string email) => new(false, NotReadyMessage);

        public AuthResult ResetPassword(string email, string verificationCode, string newPassword) =>
            new(false, NotReadyMessage);
    }
}
