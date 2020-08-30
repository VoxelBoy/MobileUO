using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    public class RenderTarget2D : Texture2D
    {
        public RenderTarget2D( GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice)
        {
            Width = width;
            Height = height;
            UnityTexture = new RenderTexture(width, height, 24)
            {
                filterMode = defaultFilterMode,
                wrapMode = TextureWrapMode.Clamp
            };
        }
    }
}
