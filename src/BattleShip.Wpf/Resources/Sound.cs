using System;
using System.IO;
using System.Media;

namespace BattleShip.Wpf.Resources
{
    public static class Sound
    {
        public static bool IsSoundEnabled { get; set; } = true;

        public static readonly string SoundFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds");

        static Sound()
        {
            DefaultSound.EnsureCreated();
        }

        public static void PlayHit() => PlayWav("hit.wav");

        public static void PlayMiss() => PlayWav("miss.wav");

        public static void PlaySunk() => PlayWav("sunk.wav");

        private static void PlayWav(string wavFileName)
        {
            if (!IsSoundEnabled) return;

            try
            {
                string wavPath = Path.Combine(SoundFolder, wavFileName);
                if (File.Exists(wavPath))
                {
                    var player = new SoundPlayer(wavPath);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}