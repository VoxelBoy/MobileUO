using System;

namespace Microsoft.Xna.Framework.Graphics
{
    
    public class DynamicIndexBuffer : IndexBuffer
    {
        public DynamicIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int maxIndices, BufferUsage writeOnly)
        {   
            
        }
    }
    public class IndexBuffer : GraphicsResource
    {
        public IndexBuffer()
        {
            
        }
        public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize sixteenBits, int maxIndices, BufferUsage writeOnly)
        {
        }

        public void SetData(short[] generateIndexArray)
        {
        }

        public void Dispose()
        {
        }

        public void SetDataPointerEXT(int i, IntPtr indicesBufferPtr, int indicesBufferLength, SetDataOptions none)
        {
            throw new NotImplementedException();
        }
    }
}