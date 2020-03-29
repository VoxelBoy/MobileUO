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

namespace Microsoft.Xna.Framework
{
    public class GameTime
    {
        private TimeSpan elapsedGameTime;
        private TimeSpan elapsedRealTime;
        private bool isRunningSlowly;
        private TimeSpan totalGameTime;
        private TimeSpan totalRealTime;

        public GameTime()
        {
        }

        public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, TimeSpan totalGameTime, TimeSpan elapsedGameTime)
            : this(totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, false)
        {
        }

        public GameTime(TimeSpan totalRealTime, TimeSpan elapsedRealTime, TimeSpan totalGameTime, TimeSpan elapsedGameTime, bool isRunningSlowly)
        {
            this.totalRealTime = totalRealTime;
            this.elapsedRealTime = elapsedRealTime;
            this.totalGameTime = totalGameTime;
            this.elapsedGameTime = elapsedGameTime;
            this.isRunningSlowly = isRunningSlowly;
        }

        public TimeSpan ElapsedGameTime
        {
            get { return elapsedGameTime; }
            internal set { elapsedGameTime = value; }
        }

        public TimeSpan ElapsedRealTime
        {
            get { return elapsedRealTime; }
            internal set { elapsedRealTime = value; }
        }

        public bool IsRunningSlowly
        {
            get { return isRunningSlowly; }
            internal set { isRunningSlowly = value; }
        }

        public TimeSpan TotalGameTime
        {
            get { return totalGameTime; }
            internal set { totalGameTime = value; }
        }

        public TimeSpan TotalRealTime
        {
            get { return totalRealTime; }
            internal set { totalRealTime = value; }
        }
    }
}