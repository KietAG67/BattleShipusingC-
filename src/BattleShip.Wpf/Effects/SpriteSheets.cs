using System.Windows;
using System.Windows.Media.Imaging;
using BattleShip.Wpf.Helpers;

namespace BattleShip.Wpf.Effects
{
    public sealed class SpriteSheet
    {
        public SpriteSheet(BitmapSource[] frames, double fps, bool loop)
        {
            Frames = frames;
            Fps = fps;
            Loop = loop;
        }

        public BitmapSource[] Frames { get; }

        public double Fps { get; }

        public bool Loop { get; }
    }

    /// <summary>
    /// Thông số hiệu ứng lấy từ Assets/images/fx/note.txt:
    /// fire 6 frame 12 FPS lặp; explosion 9 frame 15 FPS chạy 1 lần;
    /// splash 4 frame 10 FPS chạy 1 lần; missile 4 frame 15 FPS.
    /// Mỗi file trong Assets/Fx là một dải ngang gồm các frame bằng nhau (đã tách nền trong suốt).
    /// </summary>
    public static class SpriteSheets
    {
        private static readonly Dictionary<string, SpriteSheet> Cache = new();

        public static SpriteSheet? Get(string key)
        {
            if (Cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var sheet = key switch
            {
                "fire" => Load("fire.png", 6, 12, loop: true),
                "explosion" => Load("explosion.png", 9, 15, loop: false),
                "splash" => Load("splash.png", 4, 10, loop: false),
                "missile" => Load("missile.png", 4, 15, loop: true),
                // Vệt nước tĩnh để đánh dấu ô bắn trượt sau khi splash chạy xong (frame đầu của splash).
                "ripple" => Load("splash.png", 4, 1, loop: true, firstFrameOnly: true),
                _ => null,
            };

            if (sheet != null)
            {
                Cache[key] = sheet;
            }

            return sheet;
        }

        private static SpriteSheet Load(string file, int frameCount, double fps, bool loop, bool firstFrameOnly = false)
        {
            var strip = AssetImages.Get($"Fx/{file}");
            var frameWidth = strip.PixelWidth / frameCount;
            var count = firstFrameOnly ? 1 : frameCount;

            var frames = new BitmapSource[count];
            for (var i = 0; i < count; i++)
            {
                var frame = new CroppedBitmap(strip, new Int32Rect(i * frameWidth, 0, frameWidth, strip.PixelHeight));
                frame.Freeze();
                frames[i] = frame;
            }

            return new SpriteSheet(frames, fps, loop);
        }
    }
}
