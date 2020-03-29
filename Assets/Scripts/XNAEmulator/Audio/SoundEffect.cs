using System;
using Microsoft.Xna.Framework.Media;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Microsoft.Xna.Framework.Audio
{
    public class DynamicSoundEffectInstance : IDisposable
    {
        public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
        {
        }

        private AudioSource source;
        public AudioClip Clip { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }

        public bool IsDisposed { get; protected set; }

        public SoundState State { get; protected set; }

        private int pcmPos;
        private bool loop;

        public event EventHandler<EventArgs> BufferNeeded;
        private const int NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK = 0x8000; // 32768 bytes, about 0.9 seconds
        private float[] buffer;
        private float[] conversionBuffer = new float[NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK / 2];
        private int bufferPosition;
        private int bufferEndPosition;
        private bool stream;

        public void Play(string name)
        {
            Clip.name = name;
            if (stream)
            {
                source = new GameObject("Music: " + name).AddComponent<AudioSource>();
                source.clip = Clip;
                source.loop = loop;
                source.Play();
            }
            else
            {
                MediaPlayer.AudioSourceOneShot.PlayOneShot(Clip);
                Object.Destroy(Clip, Clip.length);
            }

            State = SoundState.Playing;
        }

        public void Stop()
        {
            if (stream)
            {
                source.Stop();
            }
            else
            {
                //NOTE: Should we call stop on one shot? Does it even work?
                MediaPlayer.AudioSourceOneShot.Stop();
            }
            State = SoundState.Stopped;
        }

        public void SubmitBuffer(byte[] newBuffer, bool stream, int frequency = 22050)
        {
            this.stream = stream;
            if (buffer == null)
            {
                if (stream)
                {
                    buffer = new float[NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK];
                    var floatDataLength = buffer.Length / 2;
                    ConvertByteToFloat16(newBuffer, conversionBuffer);
                    Array.Copy(conversionBuffer, 0, buffer, 0, floatDataLength);
                    bufferEndPosition = floatDataLength;
                    loop = true;
                    Clip = AudioClip.Create("clip", newBuffer.Length, 2, frequency, true, PcmRead, PcmSet);
                }
                else
                {
                    Clip = AudioClip.Create("clip", newBuffer.Length, 2, frequency, false);
                    Clip.SetData(ConvertByteToFloat16(newBuffer), 0);
                }
            }
            else
            {
                var bufferDataLength = bufferEndPosition - bufferPosition;
                Array.Copy(buffer, bufferPosition, buffer, 0, bufferDataLength);
                bufferPosition = 0;
                ConvertByteToFloat16(newBuffer, conversionBuffer);
                var floatDataLength = buffer.Length / 2;
                Array.Copy(conversionBuffer, 0, buffer, bufferDataLength, floatDataLength);
                bufferEndPosition = bufferDataLength + floatDataLength;
            }
        }

        private void PcmSet(int position)
        {
            pcmPos = position;
        }

        private void PcmRead(float[] data)
        {
            Array.Copy(buffer, bufferPosition, data, 0, data.Length);
            bufferPosition += data.Length;
            if (bufferEndPosition - bufferPosition < buffer.Length * 0.5f)
            {
                BufferNeeded?.Invoke(this, EventArgs.Empty);
            }
        }

        private static float[] ConvertByteToFloat16(byte[] array)
        {
            var length = array.Length / 2;
            float[] floatArr = new float[length];
            for (int i = 0; i < length; i++)
            {
                floatArr[i] = BitConverter.ToInt16(array, i * 2) / 32768.0f;
            }

            return floatArr;
        }

        private static void ConvertByteToFloat16(byte[] array, float[] destination)
        {
            int length = array.Length / 2;
            for (int i = 0; i < length; i++)
            {
                destination[i] = BitConverter.ToInt16(array, i * 2) / 32768.0f;
            }
        }

        public void Dispose()
        {
            if (source != null && source.gameObject != null)
            {
                Object.Destroy(source.gameObject);
            }
        }

        public enum SoundState
        {
            Playing,
            Paused,
            Stopped,
        }
    }
}
