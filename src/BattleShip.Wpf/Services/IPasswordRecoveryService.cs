namespace BattleShip.Wpf.Services
{
    /// <summary>
    /// Luồng quên mật khẩu: gửi mã xác minh qua email rồi đặt lại mật khẩu.
    /// Cần cột Email và bảng mã xác minh trong database (phần của Sỹ).
    /// Hiện LoginViewModel dùng PendingPasswordRecoveryService; khi Sỹ xong thì thay bằng lớp thật.
    /// </summary>
    public interface IPasswordRecoveryService
    {
        AuthResult SendVerificationCode(string email);

        AuthResult ResetPassword(string email, string verificationCode, string newPassword);
    }
}
