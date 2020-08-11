using System;
using System.IO;
using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : GraphicsResource, IDisposable
    {
        //This hash doesn't work as intended since it's not based on the contents of the UnityTexture but its instanceID
        //which will be different as old textures are discarded and new ones are created 
        public Texture UnityTexture { get; protected set; }

        public static FilterMode defaultFilterMode = FilterMode.Point;

        protected Texture2D(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice)
        {
            Width = width;
            Height = height;
            UnityMainThreadDispatcher.Dispatch(InitTexture);
        }

        private void InitTexture()
        {
            UnityTexture = new UnityEngine.Texture2D(Width, Height, TextureFormat.RGBA32, false, false);
            UnityTexture.filterMode = defaultFilterMode;
            UnityTexture.wrapMode = TextureWrapMode.Clamp;
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool v, SurfaceFormat surfaceFormat) :
            this(graphicsDevice, width, height)
        {
        }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public bool IsDisposed { get; private set; }

        public override void Dispose()
        {
            if (UnityTexture != null)
            {
                if (UnityTexture is RenderTexture renderTexture)
                {
                    renderTexture.Release();
                }
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    UnityEngine.Object.Destroy(UnityTexture);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(UnityTexture);
                }
#else
                UnityEngine.Object.Destroy(UnityTexture);
#endif
            }
            UnityTexture = null;
            IsDisposed = true;
        }

        private byte[] tempByteData;

        internal void SetData(byte[] data)
        {
            tempByteData = data;
            UnityMainThreadDispatcher.Dispatch(SetDataBytes);
        }

        private void SetDataBytes()
        {
            var dataLength = tempByteData.Length;
            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<byte>();
            var tmp = new byte[dataLength];
            var textureBytesWidth = Width * 4;
            var textureBytesHeight = Height;

            for (int i = 0; i < dataLength; i++)
            {
                int x = i % textureBytesWidth;
                int y = i / textureBytesWidth;
                y = textureBytesHeight - y - 1;
                var index = y * textureBytesWidth + x;
                var colorByte = tempByteData[index];
                tmp[i] = colorByte;
            }
            
            dst.CopyFrom(tmp);
            destText.Apply();
            tempByteData = null;
        }

        private Color[] tempColorData;

        internal void SetData(Color[] data)
        {
            tempColorData = data;
            UnityMainThreadDispatcher.Dispatch(SetDataColor);
        }

        private void SetDataColor()
        {
            var dataLength = tempColorData.Length;
            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var tmp = new uint[dataLength];
            var textureWidth = Width;

            for (int i = 0; i < dataLength; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                var index = y * textureWidth + (textureWidth - x - 1);
                var color = tempColorData[dataLength - index - 1];
                tmp[i] = color.PackedValue;
            }
            
            dst.CopyFrom(tmp);
            destText.Apply();
            tempColorData = null;
        }

        private uint[] tempUIntData;
        private int tempStartOffset;
        private int tempElementCount;
        private bool tempInvertY;

        internal void SetData(uint[] data, int startOffset = 0, int elementCount = 0, bool invertY = false)
        {
            tempUIntData = data;
            tempStartOffset = startOffset;
            tempElementCount = elementCount;
            tempInvertY = invertY;
            UnityMainThreadDispatcher.Dispatch(SetDataUInt);
        }

        private void SetDataUInt()
        {
            var textureWidth = Width;
            var textureHeight = Height;

            if (tempElementCount == 0)
            {
                tempElementCount = tempUIntData.Length;
            }

            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var dstLength = dst.Length;
            var tmp = new uint[dstLength];

            for (int i = 0; i < tempElementCount; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                if (tempInvertY)
                {
                    y = textureHeight - y - 1;
                }
                var index = y * textureWidth + (textureWidth - x - 1);
                if (index < tempElementCount && i < dstLength)
                {
                    tmp[i] = tempUIntData[tempElementCount + tempStartOffset - index - 1];
                }
            }
            
            dst.CopyFrom(tmp);
            destText.Apply();

            tempUIntData = null;
        }

        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            Console.WriteLine("Texture2D.FromStream is not implemented yet.");
            if (!UnityMainThreadDispatcher.IsMainThread())
                return null;
            var texture = new Texture2D(graphicsDevice, 2, 2);
            return texture;

        }

        public void SetDataPointerEXT(int level, Rectangle rectangle, IntPtr data, int dataLength)
        {
            Console.WriteLine("Texture2D.SetDataPointerEXT is not implemented yet.");
        }
    }
}