#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace InkJam.Editor
{
    public class AudioPipelineSetup
    {
        private const int SampleRate = 44100;
        
        [MenuItem("Ink Jam/Setup Audio Pipeline")]
        public static void GenerateAudio()
        {
            string dir = "Assets/Resources/Audio";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            GenerateWav($"{dir}/slide.wav", GenerateSlideWaveform(0.15f));
            GenerateWav($"{dir}/exit.wav", GenerateSplashWaveform(0.2f));
            GenerateWav($"{dir}/bleed.wav", GenerateBleedWaveform(0.3f));
            GenerateWav($"{dir}/win.wav", GenerateWinWaveform(1.0f));
            GenerateWav($"{dir}/fail.wav", GenerateFailWaveform(1.0f));

            AssetDatabase.Refresh();
            Debug.Log("Procedural Placeholder SFX generated in Assets/Resources/Audio");
        }

        private static float[] GenerateSlideWaveform(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float freq = Mathf.Lerp(200f, 600f, t / duration); // Ascending sweep
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * (1f - (t/duration)) * 0.3f; // Fade out
            }
            return data;
        }

        private static float[] GenerateSplashWaveform(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                // White noise burst
                data[i] = Random.Range(-1f, 1f) * (1f - (t / duration)) * 0.5f; 
            }
            return data;
        }

        private static float[] GenerateBleedWaveform(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float freq = Mathf.Lerp(80f, 40f, t / duration); // Low descending thud
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * (1f - (t / duration)) * 0.6f;
            }
            return data;
        }

        private static float[] GenerateWinWaveform(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            // Major chord: C4(261.63), E4(329.63), G4(392.00)
            float[] freqs = { 261.63f, 329.63f, 392.00f };
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float val = 0f;
                foreach (var f in freqs) val += Mathf.Sin(2 * Mathf.PI * f * t);
                data[i] = (val / freqs.Length) * (1f - (t / duration)) * 0.5f;
            }
            return data;
        }

        private static float[] GenerateFailWaveform(float duration)
        {
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float freq = Mathf.Lerp(150f, 50f, t / duration); // Sad descending tone
                // Sawtoothish
                data[i] = Mathf.Repeat(t * freq, 1f) * 2f - 1f;
                data[i] *= (1f - (t / duration)) * 0.4f;
            }
            return data;
        }

        private static void GenerateWav(string path, float[] samples)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                int hz = SampleRate;
                int channels = 1;
                
                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + samples.Length * 2);
                bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((short)1);
                bw.Write((short)channels);
                bw.Write(hz);
                bw.Write(hz * channels * 2);
                bw.Write((short)(channels * 2));
                bw.Write((short)16);
                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(samples.Length * 2);

                int maxVal = short.MaxValue;
                foreach (float sample in samples)
                {
                    bw.Write((short)(sample * maxVal));
                }
            }
        }
    }
}
#endif
