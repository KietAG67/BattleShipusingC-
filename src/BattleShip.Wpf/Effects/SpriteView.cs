using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BattleShip.Wpf.Effects
{
    
    public sealed class SpriteView : Image
    {
        public static readonly DependencyProperty SpriteProperty =
            DependencyProperty.Register(nameof(Sprite), typeof(string), typeof(SpriteView),
                new PropertyMetadata(null, (d, _) => ((SpriteView)d).Restart()));

        public static readonly DependencyProperty NextSpriteProperty =
            DependencyProperty.Register(nameof(NextSprite), typeof(string), typeof(SpriteView),
                new PropertyMetadata(null));

        private readonly Stopwatch _clock = new();
        private SpriteSheet? _sheet;
        private bool _running;
        private bool _nextUsed;

        public SpriteView()
        {
            Stretch = Stretch.Uniform;
            IsHitTestVisible = false;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            IsVisibleChanged += (_, e) =>
            {
                if ((bool)e.NewValue)
                {
                    Restart();
                }
                else
                {
                    Stop();
                }
            };
            Unloaded += (_, _) => Stop();
        }

        public string? Sprite
        {
            get => (string?)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        public string? NextSprite
        {
            get => (string?)GetValue(NextSpriteProperty);
            set => SetValue(NextSpriteProperty, value);
        }

        private void Restart()
        {
            Stop();
            _nextUsed = false;
            _sheet = Sprite is null ? null : SpriteSheets.Get(Sprite);

            if (_sheet == null)
            {
                Source = null;
                return;
            }

            Source = _sheet.Frames[0];
            if (IsVisible)
            {
                Play();
            }
        }

        private void Play()
        {
            _clock.Restart();

            
            if (_sheet == null || _sheet.Frames.Length <= 1 || _running)
            {
                return;
            }

            _running = true;
            CompositionTarget.Rendering += OnRendering;
        }

        private void Stop()
        {
            if (!_running)
            {
                return;
            }

            _running = false;
            CompositionTarget.Rendering -= OnRendering;
            _clock.Stop();
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var sheet = _sheet;
            if (sheet == null)
            {
                Stop();
                return;
            }

            var count = sheet.Frames.Length;
            var index = (int)(_clock.Elapsed.TotalSeconds * sheet.Fps);

            if (index >= count)
            {
                if (sheet.Loop)
                {
                    index %= count;
                }
                else
                {
                    if (!_nextUsed && NextSprite is { } nextKey && SpriteSheets.Get(nextKey) is { } next)
                    {
                        _nextUsed = true;
                        _sheet = next;
                        Source = next.Frames[0];
                        if (next.Frames.Length <= 1)
                        {
                            Stop();
                        }
                        else
                        {
                            _clock.Restart();
                        }

                        return;
                    }

                    Source = sheet.Frames[count - 1];
                    Stop();
                    return;
                }
            }

            var frame = sheet.Frames[index];
            if (!ReferenceEquals(Source, frame))
            {
                Source = frame;
            }
        }
    }
}
