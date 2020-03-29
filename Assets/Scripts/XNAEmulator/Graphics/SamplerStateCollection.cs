using Microsoft.Xna.Framework.Graphics;

namespace XNAEmulator.Graphics
{
    public sealed class SamplerStateCollection
    {
        private readonly SamplerState[] samplers;
        private readonly bool[] modifiedSamplers;

        public SamplerState this[int index]
        {
            get
            {
                return this.samplers[index];
            }
            set
            {
                this.samplers[index] = value;
                this.modifiedSamplers[index] = true;
            }
        }

        internal SamplerStateCollection(int slots, bool[] modSamplers)
        {
            this.samplers = new SamplerState[slots];
            this.modifiedSamplers = modSamplers;
            for (int index = 0; index < this.samplers.Length; ++index)
                this.samplers[index] = SamplerState.LinearWrap;
        }
    }
}