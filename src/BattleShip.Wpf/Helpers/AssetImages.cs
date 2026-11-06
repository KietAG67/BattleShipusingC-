using System.Windows.Media.Imaging;

namespace BattleShip.Wpf.Helpers
{
    
    public static class AssetImages
    {
        private static readonly Dictionary<string, BitmapImage> Cache = new();

        
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
