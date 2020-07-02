using UnityEngine;
using XNAEmulator.Graphics;
using UnityGraphics = UnityEngine.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDevice
    {
        private readonly bool[] modifiedSamplers = new bool[16];

        public GraphicsDevice()
        {
            // TODO: Complete member initialization
            //NOTE: For now, just assume 16 slots are fine instead of trying to find GLDevice.MaxTextureSlots equivalent in Unity
            //int slots1 = Math.Min(this.GLDevice.MaxTextureSlots, 16);
            int slots1 = 16;
            this.SamplerStates = new SamplerStateCollection(slots1, this.modifiedSamplers);
            Viewport = new Viewport(0, 0, Screen.width, Screen.height);
        }

        public Viewport Viewport { get; set; }

        public Rectangle ScissorRectangle { get; set; }
        public Color BlendFactor { get; set; }
        public BlendState BlendState { get; set; }
        public DepthStencilState DepthStencilState { get; set; }
        public RasterizerState RasterizerState { get; set; }
        public Texture2D[] Textures = new Texture2D[3];
        public SamplerStateCollection SamplerStates { get; }
        public IndexBuffer Indices { get; set; }

        internal void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget != null)
            {
                UnityEngine.Graphics.SetRenderTarget(renderTarget.UnityTexture as RenderTexture);
                GL.LoadPixelMatrix(0, renderTarget.Width, renderTarget.Height, 0);
            }
            else
            {
                UnityEngine.Graphics.SetRenderTarget(null);
                GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
            }
        }

        internal void Clear(Color color)
        {
            var unityColor = new UnityEngine.Color((float)color.R / 255, (float)color.G / 255, (float)color.B / 255, (float)color.A / 255);
            GL.Clear(true, color != Color.Transparent, unityColor);
        }

        public void Clear(ClearOptions options, Vector4 color, int depth, int stencil)
        {
            GL.Clear(depth != 0, color != Vector4.Zero, UnityEngine.Color.black);
        }

        public void Clear(ClearOptions options, Color color, int depth, int stencil)
        {
            GL.Clear(depth != 0 || (options & ClearOptions.DepthBuffer) != 0, color != Color.Transparent, UnityEngine.Color.black);
        }

        public void SetVertexBuffer(VertexBuffer dynamicVertexBuffer)
        {
        }

        public void DrawIndexedPrimitives(PrimitiveType triangleList, int i, int i1, int i2, int i3, int i4)
        {
        }
    }
}