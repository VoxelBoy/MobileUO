using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceManager
    {
        private Game game;

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return game.GraphicsDevice;
            }
        }

        public int PreferredBackBufferHeight { get; internal set; }
        public int PreferredBackBufferWidth { get; internal set; }
        public bool IsFullScreen { get; internal set; }
        public DepthFormat PreferredDepthStencilFormat { get; set; }
        public bool SynchronizeWithVerticalRetrace { get; set; }

        public GraphicsDeviceManager(Game game)
        {
            // TODO: Complete member initialization
            this.game = game;
        }

        internal void ApplyChanges()
        {
        }
    }
}
