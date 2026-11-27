using System.Text.RegularExpressions;

namespace BattleShip.Wpf.Helpers
{
    /// <summary>Kiểm tra định dạng email phía giao diện để báo lỗi sớm.</summary>
    public static class EmailValidator
    {
        private static readonly Regex Pattern = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        public static bool IsValid(string? email)
        {
            return !string.IsNullOrWhiteSpace(email) && Pattern.IsMatch(email.Trim());
        }
    }
}
