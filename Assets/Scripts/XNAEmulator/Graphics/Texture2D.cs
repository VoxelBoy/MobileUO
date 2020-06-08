using System;
using System.IO;
using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : IDisposable
    {
        //This hash doesn't work as intended since it's not based on the contents of the UnityTexture but its instanceID
        //which will be different as old textures are discarded and new ones are created 
        public int Hash = -1;
        public Texture UnityTexture { get; protected set; }

        private static readonly byte[] byteArray = new byte[4];

        public static FilterMode defaultFilterMode = FilterMode.Point;

        protected Texture2D()
        {
        }

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
        {
            UnityTexture = new UnityEngine.Texture2D(width, height, TextureFormat.RGBA32, false, false);
            UnityTexture.filterMode = defaultFilterMode;
            UnityTexture.wrapMode = TextureWrapMode.Clamp;
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool v, SurfaceFormat surfaceFormat) :
            this(graphicsDevice, width, height)
        {
        }

        public int Width => UnityTexture != null ? UnityTexture.width : 0;

        public int Height => UnityTexture != null ? UnityTexture.height : 0;

        public bool IsDisposed { get; private set; }

        public void Dispose()
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

        internal void SetData(byte[] data)
        {
            Console.WriteLine("SetData with byte array is not implemented.");
        }

        internal void SetData(ushort[] data)
        {
            var dataLength = data.Length;
            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var tmp = new uint[dataLength];
            var textureWidth = UnityTexture.width;

            for (int i = 0; i < dataLength; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                y *= textureWidth;
                var index = y + (textureWidth - x - 1);
                tmp[i] = (uint) u16Tou32(data[dataLength - index - 1]);
            }

            dst.CopyFrom(tmp);

            destText.Apply();

            Hash = UnityTexture.GetHashCode();
        }

        internal void SetData(Color[] data)
        {
            var dataLength = data.Length;
            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var tmp = new uint[dataLength];
            var textureWidth = UnityTexture.width;

            for (int i = 0; i < dataLength; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                y *= textureWidth;
                var index = y + (textureWidth - x - 1);
                var color = data[dataLength - index - 1];
                tmp[i] = color.PackedValue;
            }

            dst.CopyFrom(tmp);

            destText.Apply();

            Hash = UnityTexture.GetHashCode();
        }

        private static unsafe int u16Tou32(ushort color)
        {
            //Bgra5551
            if (color == 0)
                return 0;
            byte red = (byte) (((color >> 0xA) & 0x1F) * 8.225806f);
            byte green = (byte) (((color >> 0x5) & 0x1F) * 8.225806f);
            byte blue = (byte) ((color & 0x1F) * 8.225806f);
            byte alpha = (byte) ((color >> 15) * 255);
            byteArray[0] = red;
            byteArray[1] = green;
            byteArray[2] = blue;
            byteArray[3] = alpha;

            //NOTE: code below is copied from BitConverter.ToInt32
            fixed (byte* numPtr = &byteArray[0])
            {
                return *(int*) numPtr;
            }
        }

        internal void SetData(uint[] data, int startOffset = 0, int elementCount = 0, bool invertY = false)
        {
            var textureWidth = UnityTexture.width;
            var textureHeight = UnityTexture.height;

            if (elementCount == 0)
            {
                elementCount = data.Length;
            }

            var destText = UnityTexture as UnityEngine.Texture2D;
            var dst = destText.GetRawTextureData<uint>();
            var dstLength = dst.Length;
            var tmp = new uint[dstLength];

            for (int i = 0; i < elementCount; i++)
            {
                int x = i % textureWidth;
                int y = i / textureWidth;
                if (invertY)
                {
                    y = textureHeight - y - 1;
                }
                var index = (y * textureWidth) + (textureWidth - x - 1);
                if (index < elementCount && i < dstLength)
                {
                    tmp[i] = data[elementCount + startOffset - index - 1];
                }
            }

            dst.CopyFrom(tmp);

            destText.Apply();

            Hash = UnityTexture.GetHashCode();
        }

        public static void TextureDataFromStreamEXT(
            Stream stream,
            out int width,
            out int height,
            out byte[] pixels,
            int requestedWidth = -1,
            int requestedHeight = -1,
            bool zoom = false)
        {
            width = requestedWidth;
            height = requestedHeight;
            pixels = new[] {(byte)0};
        }
        
        public override int GetHashCode()
        {
            return Hash;
        }

        public static Texture2D FromStream(GraphicsDevice graphicsDevice, MemoryStream ms)
        {
            //TODO: Implement
            var texture = new Texture2D(graphicsDevice, 2, 2);
            return texture;
        }
    }
}