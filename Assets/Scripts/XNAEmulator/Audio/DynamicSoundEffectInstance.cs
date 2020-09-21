using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.Xna.Framework.Audio
{
    public class DynamicSoundEffectInstance : IDisposable
    {
        private readonly int sampleRate;
        private readonly int channels;
        
        public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
        {
            this.sampleRate = sampleRate;
            this.channels = channels == AudioChannels.Stereo ? 2 : 1;
        }

        public AudioSource source;
        private AudioClip Clip { get; set; }
        public bool IsDisposed { get; private set; }
        
        public SoundState State { get; private set; }

        private float volume = 1;
        public float Volume
        {
            set
            {
                volume = value;
                if (source != null)
                {
                    source.volume = volume;
                }
            }
        }

        private bool loop;

        public event EventHandler<EventArgs> BufferNeeded;
        private const int NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK = 0x8000; // 32768 bytes, about 0.9 seconds
        private float[] buffer;
        private float[] conversionBuffer;
        private int bufferPosition;
        private int bufferEndPosition;

        private static readonly Stack<AudioSource> audioSourcePool = new Stack<AudioSource>();
        private static GameObject cameraGameObject;

        private static AudioSource GetAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                return audioSourcePool.Pop();
            }
            
            if (cameraGameObject == null)
            {
                cameraGameObject = Camera.main.gameObject;
            }

            return cameraGameObject.AddComponent<AudioSource>();
        }
            
        public void Play(string name)
        {
            Clip.name = name;

            source = GetAudioSource();
            source.clip = Clip;
            source.loop = loop;
            source.volume = volume;
            source.Play();

            State = SoundState.Playing;
        }

        public void Stop()
        {
            UnityMainThreadDispatcher.Dispatch(RecycleSourceAndDestroyClip);
            State = SoundState.Stopped;
        }

        private void RecycleSourceAndDestroyClip()
        {
            if (source != null)
            {
                source.volume = 0.0f;
                source.Stop();
                source.clip = null;
                if (audioSourcePool.Contains(source) == false)
                {
                    audioSourcePool.Push(source);
                }
                source = null;
            }

            if (Clip != null)
            {
                UnityEngine.Object.Destroy(Clip);
                Clip = null;
            }
        }

        public void Dispose()
        {
            UnityMainThreadDispatcher.Dispatch(RecycleSourceAndDestroyClip);

            IsDisposed = true;
            
            buffer = null;
            conversionBuffer = null;
        }

        public void SubmitBuffer(byte[] newBuffer, bool stream = false, int count = 0)
        {
            if (buffer == null)
            {
                if (stream)
                {
                    //Buffer is created at double the size necessary for fitting the provided newBuffer byte array because as more data comes in, we want to append to this buffer
                    buffer = new float[NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK];
                    conversionBuffer = new float[NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK / 2];
                    var floatDataLength = buffer.Length / 2;
                    ConvertByteToFloat16(newBuffer, conversionBuffer);
                    Array.Copy(conversionBuffer, 0, buffer, 0, floatDataLength);
                    bufferEndPosition = floatDataLength;
                    loop = true;
                    Clip = AudioClip.Create("clip", newBuffer.Length, 2, sampleRate, true, PcmRead, PcmSet);
                }
                else
                {
                    Clip = AudioClip.Create("clip", newBuffer.Length, 1, sampleRate, false);
                    Clip.SetData(ConvertByteToFloat16(newBuffer), 0);
                }
            }
            else
            {
                //Append new data into the buffer
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
            // bufferPosition = position;
        }

        private void PcmRead(float[] data)
        {
            var length = Mathf.Min(bufferEndPosition - bufferPosition, data.Length);
            Array.Copy(buffer, bufferPosition, data, 0, length);
            bufferPosition += length;
            if (bufferEndPosition - bufferPosition < buffer.Length * 0.5f)
            {
                BufferNeeded?.Invoke(this, EventArgs.Empty);
            }
        }

        private static float[] ConvertByteToFloat16(byte[] array)
        {
            var floatArr = new float[array.Length / 2];
            ConvertByteToFloat16(array, floatArr);
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

        //Copied from SDL2.dll implementation
        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            sizeInBytes /= 2;
            return new TimeSpan(0,0,0,0, (int) ((double) sizeInBytes / (int) channels / ((double) sampleRate / 1000.0)));
        }

        public static void DisposePool()
        {
            foreach (var audioSource in audioSourcePool)
            {
                UnityEngine.Object.Destroy(audioSource);
            }
            audioSourcePool.Clear();
        }
    }
}
