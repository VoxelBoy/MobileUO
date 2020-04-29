using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace Microsoft.Xna.Framework
{
    public class Game : IDisposable
	{
		GraphicsDevice graphicsDevice;
        long totalTicks = 0;
        private TimeSpan INTERNAL_targetElapsedTime;
        private bool INTERNAL_isMouseVisible;
        private bool INTERNAL_isActive;
        private bool isDisposed;

        public TimeSpan TargetElapsedTime
        {
	        get
	        {
		        return this.INTERNAL_targetElapsedTime;
	        }
	        set
	        {
		        if (value <= TimeSpan.Zero)
			        throw new ArgumentOutOfRangeException("The time must be positive and non-zero.", (Exception) null);
		        this.INTERNAL_targetElapsedTime = value;
	        }
        }

        public bool IsFixedTimeStep { get; set; }

        public bool IsMouseVisible
        {
	        get
	        {
		        return this.INTERNAL_isMouseVisible;
	        }
	        set
	        {
		        if (this.INTERNAL_isMouseVisible == value)
			        return;
		        this.INTERNAL_isMouseVisible = value;
		        //FNAPlatform.OnIsMouseVisibleChanged(value);
	        }
        }

        public bool IsActive
        {
	        get
	        {
		        return this.INTERNAL_isActive;
	        }
	        internal set
	        {
		        if (this.INTERNAL_isActive == value)
			        return;
		        this.INTERNAL_isActive = value;
		        if (this.INTERNAL_isActive)
			        this.OnActivated((object) this, EventArgs.Empty);
		        else
			        this.OnDeactivated((object) this, EventArgs.Empty);
	        }
        }
		
        public GraphicsDevice GraphicsDevice
        {
            get
            {
				if(graphicsDevice == null)
					graphicsDevice = new GraphicsDevice();
				
				return graphicsDevice;
            }
        }

        public event EventHandler<EventArgs> Activated;

        public event EventHandler<EventArgs> Deactivated;

        public event EventHandler<EventArgs> Disposed;

        public event EventHandler<EventArgs> Exiting;

        public Game()
        {
			Window = new UnityGameWindow();
        }

        protected virtual void Update(GameTime gameTime)
        {  
        }

        protected virtual void Draw(GameTime gameTime)
        {
        }
        protected virtual void LoadContent()
        {
        }
        public virtual void Exit()
        {
	        Exiting?.Invoke(this, EventArgs.Empty);
        }

        // TODO
        public GameWindow Window { get; }

        internal void Run()
        {
	        Initialize();
            Begin();
        }
        protected virtual void Dispose(bool disposing)
        {
        }
        public void Dispose()
        {
	        Dispose(true);
            GC.SuppressFinalize(this);
        }



        internal void Begin()
        {
            LoadContent();
			// XNA's first update call has a zero elapsed time, so do one now.
			GameTime gameTime = new GameTime(new TimeSpan(0), new TimeSpan(0), new TimeSpan(0, 0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 0));
			Update(gameTime);
        }

        internal void Tick(float deltaTime)
        {
            long microseconds = (int)(deltaTime * 1000000);
			long ticks = microseconds * 10;
            totalTicks += ticks;
            GameTime gameTime = new GameTime(new TimeSpan(0), new TimeSpan(0), new TimeSpan(totalTicks), new TimeSpan(ticks));
            Update(gameTime);
        }

        public void DrawUnity(float delta)
        {
            long microseconds = (int)( delta * 1000000 );
            long ticks = microseconds * 10;
            GameTime gameTime = new GameTime( new TimeSpan( 0 ), new TimeSpan( 0 ), new TimeSpan( totalTicks ), new TimeSpan( ticks ) );
            Draw( gameTime );

        }

        protected virtual void OnActivated(object sender, EventArgs args)
        {
	        this.AssertNotDisposed();
	        if (this.Activated == null)
		        return;
	        this.Activated((object) this, args);
        }

        protected virtual void OnDeactivated(object sender, EventArgs args)
        {
	        this.AssertNotDisposed();
	        if (this.Deactivated == null)
		        return;
	        this.Deactivated((object) this, args);
        }

        [DebuggerNonUserCode]
        private void AssertNotDisposed()
        {
	        if (this.isDisposed)
	        {
		        string name = this.GetType().Name;
		        throw new ObjectDisposedException(name, string.Format("The {0} object was used after being Disposed.", (object) name));
	        }
        }

        protected virtual void Initialize()
        {
	        INTERNAL_isActive = true;
        }

        protected virtual void OnExiting(object sender, EventArgs args)
        {
        }

        protected virtual void BeginRun()
        {
        }

        public virtual void UnloadContent()
        {
        }

        public virtual void OnSDLEvent(ref SDL.SDL_Event ev)
        {
        }

        public void SuppressDraw()
        {
        }
	}
}
