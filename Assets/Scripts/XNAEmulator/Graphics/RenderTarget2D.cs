using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    public class RenderTarget2D : Texture2D
    {
        public RenderTarget2D( GraphicsDevice graphicsDevice, int width, int height, bool v, SurfaceFormat surfaceFormat, DepthFormat depth24Stencil8, int v1 = 0, RenderTargetUsage discardContents = RenderTargetUsage.DiscardContents)
        {
            UnityTexture = new RenderTexture( width, height,24);
            UnityTexture.filterMode = defaultFilterMode;
            UnityTexture.wrapMode = TextureWrapMode.Clamp;
            Hash = UnityTexture.GetHashCode();
        }
    }
}
