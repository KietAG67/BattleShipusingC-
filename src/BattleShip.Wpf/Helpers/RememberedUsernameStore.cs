using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace BattleShip.Wpf.Helpers
{
    /// <summary>
    /// Ghi nhớ tên đăng nhập trên máy người chơi (file JSON trong AppData\Local).
    /// Chỉ lưu tên đăng nhập, KHÔNG lưu mật khẩu.
    /// </summary>
    public static class RememberedUsernameStore
    {
        private const string FolderName = "BattleShip";
        private const string FileName = "remembered-user.json";

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            FolderName,
            FileName);

        /// <summary>Tên đã ghi nhớ; null nếu chưa có hoặc file không đọc được.</summary>
        public static string? Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return null;
                }

                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<RememberedUser>(json)?.Username;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                Trace.TraceWarning(ex.ToString());
                return null;
            }
        }

        public static void Save(string username)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(new RememberedUser(username)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        public sealed record RememberedUser(string? Username);
    }
}
