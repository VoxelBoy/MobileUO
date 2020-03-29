using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Audio
{
    [Serializable]
    public sealed class NoAudioHardwareException : ExternalException
    {
        public NoAudioHardwareException()
        {
        }

        public NoAudioHardwareException(string message)
            : base(message)
        {
        }

        public NoAudioHardwareException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}