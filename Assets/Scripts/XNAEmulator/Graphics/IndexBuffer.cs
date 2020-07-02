using System;

namespace Microsoft.Xna.Framework.Graphics
{
    
    public class DynamicIndexBuffer : IndexBuffer
    {
        public DynamicIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int maxIndices, BufferUsage writeOnly) : base(graphicsDevice)
        {   
            
        }
    }
    public class IndexBuffer : GraphicsResource
    {
        protected IndexBuffer(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            
        }
        public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize sixteenBits, int maxIndices, BufferUsage writeOnly) : base(graphicsDevice)
        {
        }

        public void SetData(short[] generateIndexArray)
        {
        }

        public override void Dispose()
        {
        }

        public void SetDataPointerEXT(int i, IntPtr indicesBufferPtr, int indicesBufferLength, SetDataOptions none)
        {
            throw new NotImplementedException();
        }
    }
}