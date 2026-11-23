using System;
using System.IO;
using System.Text;

namespace BattleShip.Wpf.Resources
{
    public static class DefaultSound
    {
        public static void EnsureCreated()
        {
            try
            {
                Directory.CreateDirectory(Sound.SoundFolder);

                CreateWavIfMissing(Path.Combine(Sound.SoundFolder, "hit.wav"), 0.25, (t, dur) =>
                {
                    double freq = 400 * (1 - t / dur) + 80;
                    double noise = (Random.Shared.NextDouble() * 2 - 1) * 0.5;
                    return (Math.Sin(2 * Math.PI * freq * t) * 0.5 + noise) * (1 - t / dur);
                });

                CreateWavIfMissing(Path.Combine(Sound.SoundFolder, "miss.wav"), 0.30, (t, dur) =>
                {
                    double freq = t < 0.15
                        ? 500 - (t / 0.15) * 300
                        : 200 + ((t - 0.15) / 0.15) * 250;
                    return Math.Sin(2 * Math.PI * freq * t) * 0.6 * (1 - t / dur);
                });

                CreateWavIfMissing(Path.Combine(Sound.SoundFolder, "sunk.wav"), 0.70, (t, dur) =>
                {
                    double freq = 140 * (1 - t / dur) + 40;
                    double noise = (Random.Shared.NextDouble() * 2 - 1) * 0.7;
                    return (Math.Sin(2 * Math.PI * freq * t) * 0.5 + noise) * Math.Pow(1 - t / dur, 1.5);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default sounds: {ex.Message}");
            }
        }

        private static void CreateWavIfMissing(
            string filePath,
            double durationSeconds,
            Func<double, double, double> waveFunc)
        {
            if (File.Exists(filePath)) return;

            int sampleRate = 22050;
            int numSamples = (int)(sampleRate * durationSeconds);
            int dataSize = numSamples * 2;

            using var fs = new FileStream(filePath, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataSize);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)1);
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2);
            bw.Write((short)2);
            bw.Write((short)16);
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double sample = Math.Clamp(waveFunc(t, durationSeconds), -1.0, 1.0);
                bw.Write((short)(sample * short.MaxValue));
            }
        }
    }
}