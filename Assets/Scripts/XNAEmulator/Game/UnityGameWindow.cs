using System;
using UnityEngine;

namespace Microsoft.Xna.Framework
{
    sealed class UnityGameWindow : GameWindow
	{
        public UnityGameWindow()
        {
            ClientBounds = new Rectangle(0, 0, Screen.width, Screen.height);
        }

        public override bool AllowUserResizing
        {
            get => true;
            set
            {
                
            }
        }

        public override Rectangle ClientBounds { get; set; }

        public float Scale
        {
            set => ClientBounds = new Rectangle(0, 0, Mathf.RoundToInt(Screen.width / value), Mathf.RoundToInt(Screen.height / value));
        }

        public override IntPtr Handle => IntPtr.Zero;

        protected override void SetTitle(string title)
        {
            
        }
	}
}
