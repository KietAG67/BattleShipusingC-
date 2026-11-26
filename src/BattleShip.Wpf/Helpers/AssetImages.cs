using System.Windows.Media.Imaging;

namespace BattleShip.Wpf.Helpers
{
    /// <summary>Nạp ảnh trong thư mục Assets (Build Action = Resource) và cache lại.</summary>
    public static class AssetImages
    {
        private static readonly Dictionary<string, BitmapImage> Cache = new();

        /// <param name="relativePath">Ví dụ: "Ships/carrier_h.png" hoặc "Fx/fire.png".</param>
        public static BitmapImage Get(string relativePath)
        {
            if (Cache.TryGetValue(relativePath, out var cached))
            {
                return cached;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri($"pack://application:,,,/Assets/{relativePath}", UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            Cache[relativePath] = image;
            return image;
        }
    }
}
