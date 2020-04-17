using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    public class RenderTarget2D : Texture2D
    {
        public RenderTarget2D( GraphicsDevice graphicsDevice, int width, int height, bool v, SurfaceFormat surfaceFormat, DepthFormat depth24Stencil8, int v1, RenderTargetUsage discardContents )
        {
            UnityTexture = new RenderTexture( width, height,24);
            UnityTexture.wrapMode = TextureWrapMode.Clamp;
            GraphicDevice = graphicsDevice;
            Hash = UnityTexture.GetHashCode();
        }
    }
}
