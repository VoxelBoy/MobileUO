using System;
using System.Linq;
using ClassicUO.Renderer;

namespace Microsoft.Xna.Framework.Graphics
{
    public class DynamicVertexBuffer : VertexBuffer
    {
        public DynamicVertexBuffer(GraphicsDevice graphicsDevice, Type type, int maxVertices, BufferUsage writeOnly)
        {   
            
        }
    }
    public class VertexBuffer
    {
        internal UltimaBatcher2D.PositionTextureColor4[] Data;
        internal void SetData(UltimaBatcher2D.PositionTextureColor4[] vertexInfo)
        {
            Data = vertexInfo;
        }

        public void SetDataPointerEXT(
            int offsetInBytes,
            IntPtr data,
            int dataLength,
            SetDataOptions options)
        {
        }

        public void Dispose()
        {
        }
    }
}