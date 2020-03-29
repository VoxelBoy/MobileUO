#region License
/*
MIT License
Copyright © 2006 - 2007 The Mono.Xna Team

All rights reserved.

Authors:
 * Rob Loach
 * Olivier Dufour
 * Lars Magnusson

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

using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace Microsoft.Xna.Framework.Graphics
{
    [Serializable]
    public struct Viewport
    {
        #region Fields

        private int x;
        private int y;
        private int width;
        private int height;
        private float minDepth;
        private float maxDepth;
        private Rectangle titleSafeArea;

        #endregion Fields

        #region Properties

        public float AspectRatio
        {
            get { return width / height; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public float MaxDepth
        {
            get { return maxDepth; }
            set { maxDepth = value; }
        }

        public float MinDepth
        {
            get { return minDepth; }
            set { minDepth = value; }
        }

        public Rectangle TitleSafeArea
        {
            get { return titleSafeArea; }
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public Rectangle Bounds => new Rectangle(x,y,width,height);

        #endregion Properties

		
		public Viewport (
         int x,
         int y,
         int width,
         int height
		)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;

            minDepth = 0;
            maxDepth = 0;
            titleSafeArea = new Rectangle(x, y, width, height);
		}
        #region Public Methods

        public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            //TODO Use the ref versions of methods to speed things up

            //done it here is better than use D3DVect3Project in glProject
            // because it is not the same function and not need gl for that
            /*
            Matrix:
            M =
            width/2				0			0			0
                0			-height/2		0			0
                0				0		Max-Min			0
            X + width/2		height + Y		Min			1

            M * P * V * W * S
            S = source, W=world, V=view, P=projection
            */
            //here must do projection
            Vector4 result = Vector4.Transform(source, world);
            result = Vector4.Transform(result, view);
            result = Vector4.Transform(result, projection);
            result.Z = result.Z * (this.maxDepth - this.minDepth);
            result = Vector4.Divide(result, result.W);

            Vector3 finalResult = new Vector3(result.X, result.Y, result.Z);

            finalResult.X = this.X + (1 + finalResult.X) * this.width / 2;
            finalResult.Y = this.Y + (1 - finalResult.Y) * this.height / 2;
            finalResult.Z = finalResult.Z + minDepth;
            return finalResult;
        }

        public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            //TODO Use the ref versions of methods to speed things up

            Vector4 result;
            result.X = ((source.X - this.X) * 2 / this.width) - 1;
            result.Y = 1 - ((source.Y - this.Y) * 2 / this.height);
            result.Z = source.Z - minDepth;
            if (this.maxDepth - this.minDepth == 0)
                result.Z = 0;
            else
                result.Z = result.Z / (this.maxDepth - this.minDepth);
            result.W = 1f;
            result = Vector4.Transform(result, Matrix.Invert(projection));
            result = Vector4.Transform(result, Matrix.Invert(view));
            result = Vector4.Transform(result, Matrix.Invert(world));
            result = Vector4.Divide(result, result.W);
            return new Vector3(result.X, result.Y, result.Z);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3} MinDepth:{4} MaxDepth:{5}}}", new object[] { x, y, width, height, minDepth, maxDepth });
        }

        #endregion Methods


    }
}