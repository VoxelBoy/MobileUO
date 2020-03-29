#region License

/*
MIT License
Copyright � 2006 The Mono.Xna Team

All rights reserved.

Authors:
 * Stuart Carnie

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

#region Original License

//
// System.Drawing.Color.cs
//
// Authors:
//      Dennis Hayes (dennish@raytek.com)
//      Ben Houston  (ben@exocortex.org)
//      Gonzalo Paniagua (gonzalo@ximian.com)
//      Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2002 Dennis Hayes
// (c) 2002 Ximian, Inc. (http://www.ximiam.com)
// (C) 2005 HotFeet GmbH (http://www.hotfeet.ch)
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#endregion Original License

using System;
using System.Collections;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework
{
    public struct Color : IPackedVector<uint>, IPackedVector, IEquatable<Color>
    {
        #region Private Fields

        uint _packedValue;

        #endregion Private Fields

        public Color(Vector3 vector)
        {
            _packedValue = InitializeFromVector3(vector);
        }

        public Color(Vector4 vector)
        {
            _packedValue = InitializeFromVector4(vector);
        }

        public Color(byte r, byte g, byte b)
        {
            _packedValue = InitializeFromArgb(255, r, g, b);
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            _packedValue = InitializeFromArgb(a, r, g, b);
        }

        public Color(float r, float g, float b)
        {
            byte byteR = (byte)(Math.Round(r * 255));
            byte byteG = (byte)(Math.Round(g * 255));
            byte byteB = (byte)(Math.Round(b * 255));
            _packedValue = InitializeFromArgb(255, byteR, byteG, byteB);
        }

        public Color(float r, float g, float b, float a)
        {
            byte byteR = (byte)(Math.Round(r * 255));
            byte byteG = (byte)(Math.Round(g * 255));
            byte byteB = (byte)(Math.Round(b * 255));
            byte byteA = (byte)(Math.Round(a * 255));
            _packedValue = InitializeFromArgb(byteA, byteR, byteG, byteB);
        }

        public Color(Color rgb, byte a)
        {
            _packedValue = InitializeFromArgb(a, rgb.R, rgb.G, rgb.B);
        }

        public Color(Color rgb, float a)
        {
            byte byteA = (byte)(Math.Round(a * 255));
            _packedValue = InitializeFromArgb(byteA, rgb.R, rgb.G, rgb.B);
        }

        internal Color(uint packedValue)
        {
            _packedValue = packedValue;
        }

        /// <summary>
        ///     Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///     Compares two Color objects. The return value is
        ///     based on the equivalence of the A,R,G,B properties 
        ///     of the two Colors.
        /// </remarks>
        public static bool operator ==(Color colorA, Color colorB)
        {
            return colorA._packedValue == colorB._packedValue;
        }

        /// <summary>
        ///     Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///     Compares two Color objects. The return value is
        ///     based on the equivalence of the A,R,G,B properties 
        ///     of the two colors.
        /// </remarks>
        public static bool operator !=(Color colorA, Color colorB)
        {
            return !(colorA == colorB);
        }

        #region Internal Members

        internal float GetBrightness()
        {
            byte minval = Math.Min(R, Math.Min(G, B));
            byte maxval = Math.Max(R, Math.Max(G, B));

            return (float)(maxval + minval) / 510;
        }

        internal float GetSaturation()
        {
            byte minval = (byte)Math.Min(R, Math.Min(G, B));
            byte maxval = (byte)Math.Max(R, Math.Max(G, B));

            if (maxval == minval)
                return 0.0f;

            int sum = maxval + minval;
            if (sum > 255)
                sum = 510 - sum;

            return (float)(maxval - minval) / sum;
        }

        internal float GetHue()
        {
            int r = R;
            int g = G;
            int b = B;
            byte minval = (byte)Math.Min(r, Math.Min(g, b));
            byte maxval = (byte)Math.Max(r, Math.Max(g, b));

            if (maxval == minval)
                return 0.0f;

            float diff = (float)(maxval - minval);
            float rnorm = (maxval - r) / diff;
            float gnorm = (maxval - g) / diff;
            float bnorm = (maxval - b) / diff;

            float hue = 0.0f;
            if (r == maxval)
                hue = 60.0f * (6.0f + bnorm - gnorm);
            if (g == maxval)
                hue = 60.0f * (2.0f + rnorm - bnorm);
            if (b == maxval)
                hue = 60.0f * (4.0f + gnorm - rnorm);
            if (hue > 360.0f)
                hue = hue - 360.0f;

            return hue;
        }

        #endregion Internal Members

        #region Public Members

        /// <summary>
        ///     A Property
        /// </summary>
        ///
        /// <remarks>
        ///     The transparancy of the Color.
        /// </remarks>
        public byte A
        {
            get { return (byte)((_packedValue >> 24 & 0xff)); }
            set { _packedValue = InitializeFromArgb(value, R, G, B); }
        }

        /// <summary>
        ///     R Property
        /// </summary>
        ///
        /// <remarks>
        ///     The red value of the Color.
        /// </remarks>
        public byte R
        {
            get { return (byte)((_packedValue >> 16 & 0xff)); }
            set { _packedValue = InitializeFromArgb(A, value, G, B); }
        }

        /// <summary>
        ///     G Property
        /// </summary>
        ///
        /// <remarks>
        ///     The green value of the Color.
        /// </remarks>
        public byte G
        {
            get { return (byte)((_packedValue >> 8 & 0xff)); }
            set { _packedValue = InitializeFromArgb(A, R, value, B); }
        }

        /// <summary>
        ///     B Property
        /// </summary>
        ///
        /// <remarks>
        ///     The blue value of the Color.
        /// </remarks>
        public byte B
        {
            get { return (byte)(_packedValue & 0xff); }
            set { _packedValue = InitializeFromArgb(A, R, G, value); }
        }

        /// <summary>
        ///     Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///     Checks equivalence of this Color and another object.
        /// </remarks>
        public override bool Equals(object o)
        {
            if (!(o is Color))
                return false;
            Color c = (Color)o;
            return this == c;
        }

        /// <summary>
        ///     GetHashCode Method
        /// </summary>
        ///
        /// <remarks>
        ///     Calculates a hashing value.
        /// </remarks>
        public override int GetHashCode()
        {
            return (int)_packedValue;
        }

        /// <summary>
        ///     ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///     Formats the Color as a string in ARGB notation.
        /// </remarks>
        public override string ToString()
        {
            return String.Format("{{R:{0} G:{1} B:{2} A:{3}}}", R, G, B, A);
        }

        #endregion Public Members

        #region Static Color Values

        public static Color Transparent
        {
            get { return new Color(((uint)0 << 24) + ((uint)255 << 16) + ((uint)255 << 8) + 255); }
        }

        /// <summary>Gets a system-defined color.</summary>
        /// <returns>A system-defined color.</returns>
        public static Color TransparentBlack
        {
            get { return Transparent; }
        }

        /// <summary>Gets a system-defined color.</summary>
        /// <returns>A system-defined color.</returns>
        public static Color TransparentWhite
        {
            get { return Transparent; }
        }

        public static Color AliceBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)240 << 16) + ((uint)248 << 8) + 255); }
        }

        public static Color AntiqueWhite
        {
            get { return new Color(((uint)255 << 24) + ((uint)250 << 16) + ((uint)235 << 8) + 215); }
        }

        public static Color Aqua
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)255 << 8) + 255); }
        }

        public static Color Aquamarine
        {
            get { return new Color(((uint)255 << 24) + ((uint)127 << 16) + ((uint)255 << 8) + 212); }
        }

        public static Color Azure
        {
            get { return new Color(((uint)255 << 24) + ((uint)240 << 16) + ((uint)255 << 8) + 255); }
        }

        public static Color Beige
        {
            get { return new Color(((uint)255 << 24) + ((uint)245 << 16) + ((uint)245 << 8) + 220); }
        }

        public static Color Bisque
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)228 << 8) + 196); }
        }

        public static Color Black
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)0 << 8) + 0); }
        }

        public static Color BlanchedAlmond
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)235 << 8) + 205); }
        }

        public static Color Blue
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)0 << 8) + 255); }
        }

        public static Color BlueViolet
        {
            get { return new Color(((uint)255 << 24) + ((uint)138 << 16) + ((uint)43 << 8) + 226); }
        }

        public static Color Brown
        {
            get { return new Color(((uint)255 << 24) + ((uint)165 << 16) + ((uint)42 << 8) + 42); }
        }

        public static Color BurlyWood
        {
            get { return new Color(((uint)255 << 24) + ((uint)222 << 16) + ((uint)184 << 8) + 135); }
        }

        public static Color CadetBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)95 << 16) + ((uint)158 << 8) + 160); }
        }

        public static Color Chartreuse
        {
            get { return new Color(((uint)255 << 24) + ((uint)127 << 16) + ((uint)255 << 8) + 0); }
        }

        public static Color Chocolate
        {
            get { return new Color(((uint)255 << 24) + ((uint)210 << 16) + ((uint)105 << 8) + 30); }
        }

        public static Color Coral
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)127 << 8) + 80); }
        }

        public static Color CornflowerBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)100 << 16) + ((uint)149 << 8) + 237); }
        }

        public static Color Cornsilk
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)248 << 8) + 220); }
        }

        public static Color Crimson
        {
            get { return new Color(((uint)255 << 24) + ((uint)220 << 16) + ((uint)20 << 8) + 60); }
        }

        public static Color Cyan
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)255 << 8) + 255); }
        }

        public static Color DarkBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)0 << 8) + 139); }
        }

        public static Color DarkCyan
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)139 << 8) + 139); }
        }

        public static Color DarkGoldenrod
        {
            get { return new Color(((uint)255 << 24) + ((uint)184 << 16) + ((uint)134 << 8) + 11); }
        }

        public static Color DarkGray
        {
            get { return new Color(((uint)255 << 24) + ((uint)169 << 16) + ((uint)169 << 8) + 169); }
        }

        public static Color DarkGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)100 << 8) + 0); }
        }

        public static Color DarkKhaki
        {
            get { return new Color(((uint)255 << 24) + ((uint)189 << 16) + ((uint)183 << 8) + 107); }
        }

        public static Color DarkMagenta
        {
            get { return new Color(((uint)255 << 24) + ((uint)139 << 16) + ((uint)0 << 8) + 139); }
        }

        public static Color DarkOliveGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)85 << 16) + ((uint)107 << 8) + 47); }
        }

        public static Color DarkOrange
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)140 << 8) + 0); }
        }

        public static Color DarkOrchid
        {
            get { return new Color(((uint)255 << 24) + ((uint)153 << 16) + ((uint)50 << 8) + 204); }
        }

        public static Color DarkRed
        {
            get { return new Color(((uint)255 << 24) + ((uint)139 << 16) + ((uint)0 << 8) + 0); }
        }

        public static Color DarkSalmon
        {
            get { return new Color(((uint)255 << 24) + ((uint)233 << 16) + ((uint)150 << 8) + 122); }
        }

        public static Color DarkSeaGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)143 << 16) + ((uint)188 << 8) + 139); }
        }

        public static Color DarkSlateBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)72 << 16) + ((uint)61 << 8) + 139); }
        }

        public static Color DarkSlateGray
        {
            get { return new Color(((uint)255 << 24) + ((uint)47 << 16) + ((uint)79 << 8) + 79); }
        }

        public static Color DarkTurquoise
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)206 << 8) + 209); }
        }

        public static Color DarkViolet
        {
            get { return new Color(((uint)255 << 24) + ((uint)148 << 16) + ((uint)0 << 8) + 211); }
        }

        public static Color DeepPink
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)20 << 8) + 147); }
        }

        public static Color DeepSkyBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)191 << 8) + 255); }
        }

        public static Color DimGray
        {
            get { return new Color(((uint)255 << 24) + ((uint)105 << 16) + ((uint)105 << 8) + 105); }
        }

        public static Color DodgerBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)30 << 16) + ((uint)144 << 8) + 255); }
        }

        public static Color Firebrick
        {
            get { return new Color(((uint)255 << 24) + ((uint)178 << 16) + ((uint)34 << 8) + 34); }
        }

        public static Color FloralWhite
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)250 << 8) + 240); }
        }

        public static Color ForestGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)34 << 16) + ((uint)139 << 8) + 34); }
        }

        public static Color Fuchsia
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)0 << 8) + 255); }
        }

        public static Color Gainsboro
        {
            get { return new Color(((uint)255 << 24) + ((uint)220 << 16) + ((uint)220 << 8) + 220); }
        }

        public static Color GhostWhite
        {
            get { return new Color(((uint)255 << 24) + ((uint)248 << 16) + ((uint)248 << 8) + 255); }
        }

        public static Color Gold
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)215 << 8) + 0); }
        }

        public static Color Goldenrod
        {
            get { return new Color(((uint)255 << 24) + ((uint)218 << 16) + ((uint)165 << 8) + 32); }
        }

        public static Color Gray
        {
            get { return new Color(((uint)255 << 24) + ((uint)128 << 16) + ((uint)128 << 8) + 128); }
        }

        public static Color Green
        {
            get
            {
                // LAMESPEC: MS uses A=255, R=0, G=128, B=0 for Green Color,
                // which is seems to be wrong. G must be 255.
                return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)128 << 8) + 0);
            }
        }

        public static Color GreenYellow
        {
            get { return new Color(((uint)255 << 24) + ((uint)173 << 16) + ((uint)255 << 8) + 47); }
        }

        public static Color Honeydew
        {
            get { return new Color(((uint)255 << 24) + ((uint)240 << 16) + ((uint)255 << 8) + 240); }
        }

        public static Color HotPink
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)105 << 8) + 180); }
        }

        public static Color IndianRed
        {
            get { return new Color(((uint)255 << 24) + ((uint)205 << 16) + ((uint)92 << 8) + 92); }
        }

        public static Color Indigo
        {
            get { return new Color(((uint)255 << 24) + ((uint)75 << 16) + ((uint)0 << 8) + 130); }
        }

        public static Color Ivory
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)255 << 8) + 240); }
        }

        public static Color Khaki
        {
            get { return new Color(((uint)255 << 24) + ((uint)240 << 16) + ((uint)230 << 8) + 140); }
        }

        public static Color Lavender
        {
            get { return new Color(((uint)255 << 24) + ((uint)230 << 16) + ((uint)230 << 8) + 250); }
        }

        public static Color LavenderBlush
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)240 << 8) + 245); }
        }

        public static Color LawnGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)124 << 16) + ((uint)252 << 8) + 0); }
        }

        public static Color LemonChiffon
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)250 << 8) + 205); }
        }

        public static Color LightBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)173 << 16) + ((uint)216 << 8) + 230); }
        }

        public static Color LightCoral
        {
            get { return new Color(((uint)255 << 24) + ((uint)240 << 16) + ((uint)128 << 8) + 128); }
        }

        public static Color LightCyan
        {
            get { return new Color(((uint)255 << 24) + ((uint)224 << 16) + ((uint)255 << 8) + 255); }
        }

        public static Color LightGoldenrodYellow
        {
            get { return new Color(((uint)255 << 24) + ((uint)250 << 16) + ((uint)250 << 8) + 210); }
        }

        public static Color LightGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)144 << 16) + ((uint)238 << 8) + 144); }
        }

        public static Color LightGray
        {
            get { return new Color(((uint)255 << 24) + ((uint)211 << 16) + ((uint)211 << 8) + 211); }
        }

        public static Color LightPink
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)182 << 8) + 193); }
        }

        public static Color LightSalmon
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)160 << 8) + 122); }
        }

        public static Color LightSeaGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)32 << 16) + ((uint)178 << 8) + 170); }
        }

        public static Color LightSkyBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)135 << 16) + ((uint)206 << 8) + 250); }
        }

        public static Color LightSlateGray
        {
            get { return new Color(((uint)255 << 24) + ((uint)119 << 16) + ((uint)136 << 8) + 153); }
        }

        public static Color LightSteelBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)176 << 16) + ((uint)196 << 8) + 222); }
        }

        public static Color LightYellow
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)255 << 8) + 224); }
        }

        public static Color Lime
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)255 << 8) + 0); }
        }

        public static Color LimeGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)50 << 16) + ((uint)205 << 8) + 50); }
        }

        public static Color Linen
        {
            get { return new Color(((uint)255 << 24) + ((uint)250 << 16) + ((uint)240 << 8) + 230); }
        }

        public static Color Magenta
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)0 << 8) + 255); }
        }

        public static Color Maroon
        {
            get { return new Color(((uint)255 << 24) + ((uint)128 << 16) + ((uint)0 << 8) + 0); }
        }

        public static Color MediumAquamarine
        {
            get { return new Color(((uint)255 << 24) + ((uint)102 << 16) + ((uint)205 << 8) + 170); }
        }

        public static Color MediumBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)0 << 8) + 205); }
        }

        public static Color MediumOrchid
        {
            get { return new Color(((uint)255 << 24) + ((uint)186 << 16) + ((uint)85 << 8) + 211); }
        }

        public static Color MediumPurple
        {
            get { return new Color(((uint)255 << 24) + ((uint)147 << 16) + ((uint)112 << 8) + 219); }
        }

        public static Color MediumSeaGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)60 << 16) + ((uint)179 << 8) + 113); }
        }

        public static Color MediumSlateBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)123 << 16) + ((uint)104 << 8) + 238); }
        }

        public static Color MediumSpringGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)250 << 8) + 154); }
        }

        public static Color MediumTurquoise
        {
            get { return new Color(((uint)255 << 24) + ((uint)72 << 16) + ((uint)209 << 8) + 204); }
        }

        public static Color MediumVioletRed
        {
            get { return new Color(((uint)255 << 24) + ((uint)199 << 16) + ((uint)21 << 8) + 133); }
        }

        public static Color MidnightBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)25 << 16) + ((uint)25 << 8) + 112); }
        }

        public static Color MintCream
        {
            get { return new Color(((uint)255 << 24) + ((uint)245 << 16) + ((uint)255 << 8) + 250); }
        }

        public static Color MistyRose
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)228 << 8) + 225); }
        }

        public static Color Moccasin
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)228 << 8) + 181); }
        }

        public static Color NavajoWhite
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)222 << 8) + 173); }
        }

        public static Color Navy
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)0 << 8) + 128); }
        }

        public static Color OldLace
        {
            get { return new Color(((uint)255 << 24) + ((uint)253 << 16) + ((uint)245 << 8) + 230); }
        }

        public static Color Olive
        {
            get { return new Color(((uint)255 << 24) + ((uint)128 << 16) + ((uint)128 << 8) + 0); }
        }

        public static Color OliveDrab
        {
            get { return new Color(((uint)255 << 24) + ((uint)107 << 16) + ((uint)142 << 8) + 35); }
        }

        public static Color Orange
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)165 << 8) + 0); }
        }

        public static Color OrangeRed
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)69 << 8) + 0); }
        }

        public static Color Orchid
        {
            get { return new Color(((uint)255 << 24) + ((uint)218 << 16) + ((uint)112 << 8) + 214); }
        }

        public static Color PaleGoldenrod
        {
            get { return new Color(((uint)255 << 24) + ((uint)238 << 16) + ((uint)232 << 8) + 170); }
        }

        public static Color PaleGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)152 << 16) + ((uint)251 << 8) + 152); }
        }

        public static Color PaleTurquoise
        {
            get { return new Color(((uint)255 << 24) + ((uint)175 << 16) + ((uint)238 << 8) + 238); }
        }

        public static Color PaleVioletRed
        {
            get { return new Color(((uint)255 << 24) + ((uint)219 << 16) + ((uint)112 << 8) + 147); }
        }

        public static Color PapayaWhip
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)239 << 8) + 213); }
        }

        public static Color PeachPuff
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)218 << 8) + 185); }
        }

        public static Color Peru
        {
            get { return new Color(((uint)255 << 24) + ((uint)205 << 16) + ((uint)133 << 8) + 63); }
        }

        public static Color Pink
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)192 << 8) + 203); }
        }

        public static Color Plum
        {
            get { return new Color(((uint)255 << 24) + ((uint)221 << 16) + ((uint)160 << 8) + 221); }
        }

        public static Color PowderBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)176 << 16) + ((uint)224 << 8) + 230); }
        }

        public static Color Purple
        {
            get { return new Color(((uint)255 << 24) + ((uint)128 << 16) + ((uint)0 << 8) + 128); }
        }

        public static Color Red
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)0 << 8) + 0); }
        }

        public static Color RosyBrown
        {
            get { return new Color(((uint)255 << 24) + ((uint)188 << 16) + ((uint)143 << 8) + 143); }
        }

        public static Color RoyalBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)65 << 16) + ((uint)105 << 8) + 225); }
        }

        public static Color SaddleBrown
        {
            get { return new Color(((uint)255 << 24) + ((uint)139 << 16) + ((uint)69 << 8) + 19); }
        }

        public static Color Salmon
        {
            get { return new Color(((uint)255 << 24) + ((uint)250 << 16) + ((uint)128 << 8) + 114); }
        }

        public static Color SandyBrown
        {
            get { return new Color(((uint)255 << 24) + ((uint)244 << 16) + ((uint)164 << 8) + 96); }
        }

        public static Color SeaGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)46 << 16) + ((uint)139 << 8) + 87); }
        }

        public static Color SeaShell
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)245 << 8) + 238); }
        }

        public static Color Sienna
        {
            get { return new Color(((uint)255 << 24) + ((uint)160 << 16) + ((uint)82 << 8) + 45); }
        }

        public static Color Silver
        {
            get { return new Color(((uint)255 << 24) + ((uint)192 << 16) + ((uint)192 << 8) + 192); }
        }

        public static Color SkyBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)135 << 16) + ((uint)206 << 8) + 235); }
        }

        public static Color SlateBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)106 << 16) + ((uint)90 << 8) + 205); }
        }

        public static Color SlateGray
        {
            get { return new Color(((uint)255 << 24) + ((uint)112 << 16) + ((uint)128 << 8) + 144); }
        }

        public static Color Snow
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)250 << 8) + 250); }
        }

        public static Color SpringGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)255 << 8) + 127); }
        }

        public static Color SteelBlue
        {
            get { return new Color(((uint)255 << 24) + ((uint)70 << 16) + ((uint)130 << 8) + 180); }
        }

        public static Color Tan
        {
            get { return new Color(((uint)255 << 24) + ((uint)210 << 16) + ((uint)180 << 8) + 140); }
        }

        public static Color Teal
        {
            get { return new Color(((uint)255 << 24) + ((uint)0 << 16) + ((uint)128 << 8) + 128); }
        }

        public static Color Thistle
        {
            get { return new Color(((uint)255 << 24) + ((uint)216 << 16) + ((uint)191 << 8) + 216); }
        }

        public static Color Tomato
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)99 << 8) + 71); }
        }

        public static Color Turquoise
        {
            get { return new Color(((uint)255 << 24) + ((uint)64 << 16) + ((uint)224 << 8) + 208); }
        }

        public static Color Violet
        {
            get { return new Color(((uint)255 << 24) + ((uint)238 << 16) + ((uint)130 << 8) + 238); }
        }

        public static Color Wheat
        {
            get { return new Color(((uint)255 << 24) + ((uint)245 << 16) + ((uint)222 << 8) + 179); }
        }

        public static Color White
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)255 << 8) + 255); }
        }

        public static Color WhiteSmoke
        {
            get { return new Color(((uint)255 << 24) + ((uint)245 << 16) + ((uint)245 << 8) + 245); }
        }

        public static Color Yellow
        {
            get { return new Color(((uint)255 << 24) + ((uint)255 << 16) + ((uint)255 << 8) + 0); }
        }

        public static Color YellowGreen
        {
            get { return new Color(((uint)255 << 24) + ((uint)154 << 16) + ((uint)205 << 8) + 50); }
        }

        #endregion Static Color Values

        #region IPackedVector Members

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packedValue = InitializeFromVector4(vector);
        }

        public Vector4 ToVector4()
        {
            return new Vector4((float)R / 255, (float)G / 255, (float)B / 255, (float)A / 255);
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)R / 255, (float)G / 255, (float)B / 255);
        }

        #endregion IPackedVector Members

        #region IPackedVector<uint> Members

        /// <summary>Gets the current color as a packed value.</summary>
        /// <returns>The current color.</returns>
        public uint PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        #endregion IPackedVector<uint> Members

        public static Color Lerp(Color value1, Color value2, float amount)
        {
            return new Color(
                (byte)(Math.Round(MathHelper.Lerp(value1.R, value2.R, amount))),
                (byte)(Math.Round(MathHelper.Lerp(value1.G, value2.G, amount))),
                (byte)(Math.Round(MathHelper.Lerp(value1.B, value2.B, amount))),
                (byte)(Math.Round(MathHelper.Lerp(value1.A, value2.A, amount))));
        }

        #region IEquatable<Color> Members

        public bool Equals(Color other)
        {
            return this == other;
        }

        #endregion IEquatable<Color> Members

        #region Private Members

        static uint InitializeFromVector4(Vector4 value)
        {
            byte r = (byte)(Math.Round(value.X * 255));
            byte g = (byte)(Math.Round(value.Y * 255));
            byte b = (byte)(Math.Round(value.Z * 255));
            byte a = (byte)(Math.Round(value.W * 255));
            return ((uint)a << 24) + ((uint)r << 16) + ((uint)g << 8) + b;
        }

        static uint InitializeFromVector3(Vector3 value)
        {
            byte r = (byte)(Math.Round(value.X * 255));
            byte g = (byte)(Math.Round(value.Y * 255));
            byte b = (byte)(Math.Round(value.Z * 255));
            return ((uint)255 << 24) + ((uint)r << 16) + ((uint)g << 8) + b;
        }

        static uint InitializeFromArgb(byte a, byte r, byte g, byte b)
        {
            return (uint)(a << 24) + (uint)(r << 16) + (uint)(g << 8) + b;
        }

        #endregion Private Members
    }
}