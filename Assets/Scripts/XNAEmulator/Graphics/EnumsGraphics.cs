#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum Blend
    {
        Zero = 1,
        One = 2,
        SourceColor = 3,
        InverseSourceColor = 4,
        SourceAlpha = 5,
        InverseSourceAlpha = 6,
        DestinationAlpha = 7,
        InverseDestinationAlpha = 8,
        DestinationColor = 9,
        InverseDestinationColor = 10,
        SourceAlphaSaturation = 11,
        BothSourceAlpha = 12,
        BothInverseSourceAlpha = 13,
        BlendFactor = 14,
        InverseBlendFactor = 15,
    }

    [Flags]
    public enum BufferUsage
    {
        None = 0,
        Points = 0x40,
        WriteOnly = 8
    }

    public enum BlendFunction
    {
        Add = 1,
        Subtract = 2,
        ReverseSubtract = 3,
        Min = 4,
        Max = 5,
    }


    public enum CompareFunction
    {
        Never = 1,
        Less = 2,
        Equal = 3,
        LessEqual = 4,
        Greater = 5,
        NotEqual = 6,
        GreaterEqual = 7,
        Always = 8,
    }


    public enum StencilOperation
    {
        Keep = 1,
        Zero = 2,
        Replace = 3,
        IncrementSaturation = 4,
        DecrementSaturation = 5,
        Invert = 6,
        Increment = 7,
        Decrement = 8,
    }


    public enum CullMode
    {
        None = 1,
        CullClockwiseFace = 2,
        CullCounterClockwiseFace = 3,
    }


    public enum FillMode
    {
        Point = 1,
        WireFrame = 2,
        Solid = 3,
    }


    public enum TextureAddressMode
    {
        Border = 4,
        Clamp = 3,
        Mirror = 2,
        MirrorOnce = 5,
        Wrap = 1
    }


    public enum DepthFormat
    {
        Unknown = -1,
        Depth24Stencil8 = 48,
        Depth24Stencil8Single = 49,
        Depth24Stencil4 = 50,
        Depth24 = 51,
        Depth32 = 52,
        Depth16 = 54,
        Depth15Stencil1 = 56,
    }


    public enum SurfaceFormat
    {
        Unknown = -1,
        Color = 1,
        Bgr32 = 2,
        Bgra1010102 = 3,
        Rgba32 = 4,
        Rgb32 = 5,
        Rgba1010102 = 6,
        Rg32 = 7,
        Rgba64 = 8,
        Bgr565 = 9,
        Bgra5551 = 10,
        Bgr555 = 11,
        Bgra4444 = 12,
        Bgr444 = 13,
        Bgra2338 = 14,
        Alpha8 = 15,
        Bgr233 = 16,
        Bgr24 = 17,
        NormalizedByte2 = 18,
        NormalizedByte4 = 19,
        NormalizedShort2 = 20,
        NormalizedShort4 = 21,
        Single = 22,
        Vector2 = 23,
        Vector4 = 24,
        HalfSingle = 25,
        HalfVector2 = 26,
        HalfVector4 = 27,
        Dxt1 = 28,
        Dxt2 = 29,
        Dxt3 = 30,
        Dxt4 = 31,
        Dxt5 = 32,
        Luminance8 = 33,
        Luminance16 = 34,
        LuminanceAlpha8 = 35,
        LuminanceAlpha16 = 36,
        Palette8 = 37,
        PaletteAlpha16 = 38,
        NormalizedLuminance16 = 39,
        NormalizedLuminance32 = 40,
        NormalizedAlpha1010102 = 41,
        NormalizedByte2Computed = 42,
        VideoYuYv = 43,
        VideoUyVy = 44,
        VideoGrGb = 45,
        VideoRgBg = 46,
        Multi2Bgra32 = 47,
        Depth24Stencil8 = 48,
        Depth24Stencil8Single = 49,
        Depth24Stencil4 = 50,
        Depth24 = 51,
        Depth32 = 52,
        Depth16 = 54,
        Depth15Stencil1 = 56,
    }

    public enum RenderTargetUsage
    {
        DiscardContents,
        PlatformContents,
        PreserveContents
    }


    [Flags]
    public enum ClearOptions
    {
        Target = 1,
        DepthBuffer = 2,
        Stencil = 4,
    }


    public enum PrimitiveType
    {
        PointList = 1,
        LineList = 2,
        LineStrip = 3,
        TriangleList = 4,
        TriangleStrip = 5,
        TriangleFan = 6,
    }


    public enum IndexElementSize
    {
        SixteenBits = 0,
        ThirtyTwoBits = 1,
    }


    [Flags]
    public enum SetDataOptions
    {
        None = 0,
        NoOverwrite = 4096,
        Discard = 8192,
    }


    public enum TextureFilter
    {
        None = 0,
        Point = 1,
        Linear = 2,
        Anisotropic = 3,
        PyramidalQuad = 6,
        GaussianQuad = 7,
    }


    [Flags]
    public enum ColorWriteChannels
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 4,
        Alpha = 8,
        All = 15,
    }

    public enum VertexElementFormat
    {
        Single = 0,
        Vector2 = 1,
        Vector3 = 2,
        Vector4 = 3,
        Color = 4,
        Byte4 = 5,
        Short2 = 6,
        Short4 = 7,
        Rgba32 = 8,
        NormalizedShort2 = 9,
        NormalizedShort4 = 10,
        Rg32 = 11,
        Rgba64 = 12,
        UInt101010 = 13,
        Normalized101010 = 14,
        HalfVector2 = 15,
        HalfVector4 = 16,
        Unused = 17,
    }

    public enum VertexElementUsage
    {
        Position = 0,
        BlendWeight = 1,
        BlendIndices = 2,
        Normal = 3,
        PointSize = 4,
        TextureCoordinate = 5,
        Tangent = 6,
        Binormal = 7,
        TessellateFactor = 8,
        Color = 10,
        Fog = 11,
        Depth = 12,
        Sample = 13,
    }
}