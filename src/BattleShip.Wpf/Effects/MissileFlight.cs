using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BattleShip.Wpf.Effects
{
    
    public static class MissileFlight
    {
        private const double SpriteWidth = 96;
        private const double SpriteHeight = 80;

        public static Task PlayAsync(Canvas canvas, Point from, Point to, double seconds)
        {
            var tcs = new TaskCompletionSource();

            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var length = Math.Sqrt((dx * dx) + (dy * dy));
            if (length < 1)
            {
                return Task.CompletedTask;
            }

            var ux = dx / length;
            var uy = dy / length;
            var angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            
            var endX = to.X - (ux * SpriteWidth * 0.4);
            var endY = to.Y - (uy * SpriteWidth * 0.4);

            var move = new TranslateTransform();
            var transforms = new TransformGroup();
            
            transforms.Children.Add(new ScaleTransform(1, dx < 0 ? -1 : 1));
            transforms.Children.Add(new RotateTransform(angle));
            transforms.Children.Add(move);

            var sprite = new SpriteView
            {
                Sprite = "missile",
                Width = SpriteWidth,
                Height = SpriteHeight,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = transforms,
            };
            Canvas.SetLeft(sprite, from.X - (SpriteWidth / 2));
            Canvas.SetTop(sprite, from.Y - (SpriteHeight / 2));
            canvas.Children.Add(sprite);

            var duration = TimeSpan.FromSeconds(seconds);
            var ease = new QuadraticEase { EasingMode = EasingMode.EaseIn };
            var animX = new DoubleAnimation(0, endX - from.X, duration) { EasingFunction = ease };
            var animY = new DoubleAnimation(0, endY - from.Y, duration) { EasingFunction = ease };

            animX.Completed += (_, _) =>
            {
                canvas.Children.Remove(sprite);
                tcs.TrySetResult();
            };

            move.BeginAnimation(TranslateTransform.XProperty, animX);
            move.BeginAnimation(TranslateTransform.YProperty, animY);

            return tcs.Task;
        }
    }
}
