#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SDL2;
using static SDL2.SDL;

namespace ClassicUO
{
    unsafe class GameController : Microsoft.Xna.Framework.Game
    {
        private Scene _scene;
        private bool _dragStarted;
        private bool _ignoreNextTextInput;
        private readonly GraphicsDeviceManager _graphicDeviceManager;
        private UltimaBatcher2D _uoSpriteBatch;
        private readonly float[] _intervalFixedUpdate = new float[2];
        private double _statisticsTimer;
        private double _totalElapsed, _currentFpsTime;
        private uint _totalFrames;
        
        public UltimaBatcher2D Batcher => _uoSpriteBatch;
        public static UnityEngine.TouchScreenKeyboard TouchScreenKeyboard;

        public GameController()
        {
            _graphicDeviceManager = new GraphicsDeviceManager(this);
            // _graphicDeviceManager.PreparingDeviceSettings += (sender, e) => e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
           
            _graphicDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            _graphicDeviceManager.SynchronizeWithVerticalRetrace = false; // TODO: V-Sync option

            Window.ClientSizeChanged += WindowOnClientSizeChanged;
            Window.AllowUserResizing = true;
            Window.Title = $"ClassicUO - {CUOEnviroment.Version}";
            IsMouseVisible = Settings.GlobalSettings.RunMouseInASeparateThread;

            IsFixedTimeStep = false; // Settings.GlobalSettings.FixedTimeStep;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 250.0);
            InactiveSleepTime = TimeSpan.Zero;
        }

        public Scene Scene => _scene;
        public readonly uint[] FrameDelay = new uint[2];

        private SDL_EventFilter _filter;

        protected override void Initialize()
        {
            // if (_graphicDeviceManager.GraphicsDevice.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
            //     _graphicDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            _graphicDeviceManager.ApplyChanges();

            SetRefreshRate(Settings.GlobalSettings.FPS);
            _uoSpriteBatch = new UltimaBatcher2D(GraphicsDevice);

            _filter = new SDL_EventFilter(HandleSDLEvent);
            SDL.SDL_AddEventWatch(_filter, IntPtr.Zero);

            base.Initialize();
        }

        private readonly Texture2D[] _hues_sampler = new Texture2D[2];

        protected override void LoadContent()
        {
            base.LoadContent();

            Client.Load();

            const int TEXTURE_WIDHT = 32;
            const int TEXTURE_HEIGHT = 2048 * 1;

            uint[] buffer = new uint[TEXTURE_WIDHT * TEXTURE_HEIGHT * 2];
            HuesLoader.Instance.CreateShaderColors(buffer);


            _hues_sampler[0] = new Texture2D(
                                          GraphicsDevice,
                                          TEXTURE_WIDHT,
                                          TEXTURE_HEIGHT);
            _hues_sampler[0].SetData(buffer, 0, TEXTURE_WIDHT * TEXTURE_HEIGHT, true);



            _hues_sampler[1] = new Texture2D(
                                          GraphicsDevice,
                                          TEXTURE_WIDHT,
                                          TEXTURE_HEIGHT);
            _hues_sampler[1].SetData(buffer, TEXTURE_WIDHT * TEXTURE_HEIGHT, TEXTURE_WIDHT * TEXTURE_HEIGHT, true);




            GraphicsDevice.Textures[1] = _hues_sampler[0];
            GraphicsDevice.Textures[2] = _hues_sampler[1];

            GraphicsDevice.Textures[1].UnityTexture.filterMode = UnityEngine.FilterMode.Point;
            GraphicsDevice.Textures[2].UnityTexture.filterMode = UnityEngine.FilterMode.Point;
            
            // File.WriteAllBytes(Path.Combine(UnityEngine.Application.persistentDataPath, "hue1.png"), UnityEngine.ImageConversion.EncodeToPNG(_hues_sampler[0].UnityTexture as UnityEngine.Texture2D));
            // File.WriteAllBytes(Path.Combine(UnityEngine.Application.persistentDataPath, "hue2.png"), UnityEngine.ImageConversion.EncodeToPNG(_hues_sampler[1].UnityTexture as UnityEngine.Texture2D));

            AuraManager.CreateAuraTexture();
            UIManager.InitializeGameCursor();
            AnimatedStaticsManager.Initialize();

            SetScene(new LoginScene());
            SetWindowPositionBySettings();
        }


        public override void UnloadContent()
        {
            SDL.SDL_GetWindowBordersSize(Window.Handle, out int top, out int left, out int bottom, out int right);
            Settings.GlobalSettings.WindowPosition = new Point(Math.Max(0, Window.ClientBounds.X - left), Math.Max(0, Window.ClientBounds.Y - top));

            _scene?.Unload();
            Settings.GlobalSettings.Save();
            Plugin.OnClosing();

            ArtLoader.Instance.Dispose();
            GumpsLoader.Instance.Dispose();
            TexmapsLoader.Instance.Dispose();
            AnimationsLoader.Instance.Dispose();
            LightsLoader.Instance.Dispose();
            TileDataLoader.Instance.Dispose();
            AnimDataLoader.Instance.Dispose();
            ClilocLoader.Instance.Dispose();
            FontsLoader.Instance.Dispose();
            HuesLoader.Instance.Dispose();
            MapLoader.Instance.Dispose();
            MultiLoader.Instance.Dispose();
            MultiMapLoader.Instance.Dispose();
            ProfessionLoader.Instance.Dispose();
            SkillsLoader.Instance.Dispose();
            SoundsLoader.Instance.Dispose();
            SpeechesLoader.Instance.Dispose();
            Verdata.File?.Dispose();
            World.Map?.Destroy();

            //NOTE: My dispose related changes, see if they're still necessary
            _hues_sampler[0]?.Dispose();
            _hues_sampler[0] = null;
            _hues_sampler[1]?.Dispose();
            _hues_sampler[1] = null;
            _scene?.Dispose();
            AuraManager.Dispose();
            UIManager.Dispose();
            Texture2DCache.Dispose();
            RenderedText.Dispose();
            
            //NOTE: We force the sockets to disconnect in case they haven't already been disposed
            //This is good practice since the Client can be quit while the socket is still active
            if (NetClient.LoginSocket.IsDisposed == false)
            {
                NetClient.LoginSocket.Disconnect();
            }
            if (NetClient.Socket.IsDisposed == false)
            {
                NetClient.Socket.Disconnect();
            }

            base.UnloadContent();
        }

        [MethodImpl(256)]
        public T GetScene<T>() where T : Scene
        {
            return _scene as T;
        }

        public void SetScene(Scene scene)
        {
            _scene?.Dispose();
            _scene = scene;

            //NOTE: Added this to be able to react to scene changes, mainly for calculating render scale factor
            Client.InvokeSceneChanged();

            if (scene != null)
            {
                Window.AllowUserResizing = scene.CanResize;
                scene.Load();
            }
        }

        public void SetRefreshRate(int rate)
        {
            if (rate < Constants.MIN_FPS)
                rate = Constants.MIN_FPS;
            else if (rate > Constants.MAX_FPS)
                rate = Constants.MAX_FPS;

            FrameDelay[0] = FrameDelay[1] = (uint) (1000 / rate);
            FrameDelay[1] = FrameDelay[1] >> 1;

            Settings.GlobalSettings.FPS = rate;
            //TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / 250);

            _intervalFixedUpdate[0] = 1000.0f / rate;
            _intervalFixedUpdate[1] = 217;  // 5 FPS
        }

        public void SetWindowPosition(int x, int y)
        {
            SDL.SDL_SetWindowPosition(Window.Handle, x, y);
        }

        public GraphicsDeviceManager GraphicManager => _graphicDeviceManager;

        public void SetWindowSize(int width, int height)
        {
            //width = (int) ((double) width * Client.Game.GraphicManager.PreferredBackBufferWidth / Client.Game.Window.ClientBounds.Width);
            //height = (int) ((double) height * Client.Game.GraphicManager.PreferredBackBufferHeight / Client.Game.Window.ClientBounds.Height);

            _graphicDeviceManager.PreferredBackBufferWidth = width;
            _graphicDeviceManager.PreferredBackBufferHeight = height;
            _graphicDeviceManager.ApplyChanges();
        }

        public void SetWindowBorderless(bool borderless)
        {
            SDL_WindowFlags flags = (SDL_WindowFlags) SDL.SDL_GetWindowFlags(Window.Handle);

            if ((flags & SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0 && borderless)
            {
                return;
            }

            if ((flags & SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == 0 && !borderless)
            {
                return;
            }

            SDL.SDL_SetWindowBordered(Window.Handle, borderless ? SDL_bool.SDL_FALSE : SDL_bool.SDL_TRUE);

            SDL.SDL_GetCurrentDisplayMode(0, out SDL_DisplayMode displayMode);

            int width = displayMode.w;
            int height = displayMode.h;

            if (borderless)
            {
                SetWindowSize(width, height);
                SDL_SetWindowPosition(Window.Handle, 0, 0);
            }
            else
            {
                int top, left, bottom, right;
                SDL_GetWindowBordersSize(Window.Handle, out top, out left, out bottom, out right);
                SetWindowSize(width, height - (top - bottom));
                SetWindowPositionBySettings();
            }

            var viewport = UIManager.GetGump<WorldViewportGump>();

            if (viewport != null && ProfileManager.Current.GameWindowFullSize)
            {
                viewport.ResizeGameWindow(new Point(width, height));
                viewport.X = -5;
                viewport.Y = -5;
            }
        }

        public void MaximizeWindow()
        {
            SDL.SDL_MaximizeWindow(Window.Handle);
        }

        public bool IsWindowMaximized()
        {
            SDL.SDL_WindowFlags flags = (SDL.SDL_WindowFlags) SDL.SDL_GetWindowFlags(Window.Handle);

            return (flags & SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0;
        }

        public void RestoreWindow()
        {
            SDL.SDL_RestoreWindow(Window.Handle);
        }

        public void SetWindowPositionBySettings()
        {
            SDL_GetWindowBordersSize(Window.Handle, out int top, out int left, out int bottom, out int right);
            if (Settings.GlobalSettings.WindowPosition.HasValue)
            {
                int x = left + Settings.GlobalSettings.WindowPosition.Value.X;
                int y = top + Settings.GlobalSettings.WindowPosition.Value.Y;
                x = Math.Max(0, x);
                y = Math.Max(0, y);

                SetWindowPosition(x, y);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Profiler.InContext("OutOfContext"))
                Profiler.ExitContext("OutOfContext");

            Time.Ticks = (uint) gameTime.TotalGameTime.TotalMilliseconds;

            // Mouse.Update();
            MouseUpdate();
            OnNetworkUpdate(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
            Plugin.Tick();

            if (_scene != null && _scene.IsLoaded && !_scene.IsDestroyed)
            {
                Profiler.EnterContext("Update");
                _scene.Update(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
                Profiler.ExitContext("Update");
            }
            
            UnityInputUpdate();
            
            UIManager.Update(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);

            _totalElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
            _currentFpsTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_currentFpsTime >= 1000)
            {
                CUOEnviroment.CurrentRefreshRate = _totalFrames;

                _totalFrames = 0;
                _currentFpsTime = 0;
            }

            double x = _intervalFixedUpdate[!IsActive && ProfileManager.Current != null && ProfileManager.Current.ReduceFPSWhenInactive ? 1 : 0];

            if (_totalElapsed > x)
            {
                if (_scene != null && _scene.IsLoaded && !_scene.IsDestroyed)
                {
                    Profiler.EnterContext("FixedUpdate");
                    _scene.FixedUpdate(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
                    Profiler.ExitContext("FixedUpdate");
                }

                _totalElapsed %= x;
            }
            else
            {
                SuppressDraw();

                if (!gameTime.IsRunningSlowly)
                {
                    Thread.Sleep(1);
                }
            }

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            Profiler.EndFrame();
            Profiler.BeginFrame();

            if (Profiler.InContext("OutOfContext"))
                Profiler.ExitContext("OutOfContext");
            Profiler.EnterContext("RenderFrame");

            _totalFrames++;

            if (_scene != null && _scene.IsLoaded && !_scene.IsDestroyed)
                _scene.Draw(_uoSpriteBatch);

            UIManager.Draw(_uoSpriteBatch);

            if (World.InGame && SelectedObject.LastObject is TextObject t)
            {
                if (t.IsTextGump)
                    t.ToTopD();
                else
                    World.WorldTextManager?.MoveToTop(t);
            }

            base.Draw(gameTime);

            Profiler.ExitContext("RenderFrame");
            Profiler.EnterContext("OutOfContext");

            Plugin.ProcessDrawCmdList(GraphicsDevice);
        }


        private void OnNetworkUpdate(double totalMS, double frameMS)
        {
            if (NetClient.LoginSocket.IsDisposed && NetClient.LoginSocket.IsConnected)
                NetClient.LoginSocket.Disconnect();
            else if (!NetClient.Socket.IsConnected)
            {
                NetClient.LoginSocket.Update();
                UpdateSockeStats(NetClient.LoginSocket, totalMS);
            }
            else if (!NetClient.Socket.IsDisposed)
            {
                NetClient.Socket.Update();
                UpdateSockeStats(NetClient.Socket, totalMS);
            }
        }

        private void UpdateSockeStats(NetClient socket, double totalMS)
        {
            if (_statisticsTimer < totalMS)
            {
                socket.Statistics.Update();
                _statisticsTimer = totalMS + 500;
            }
        }

        //public override void OnSDLEvent(ref SDL_Event ev)
        //{
        //    HandleSDLEvent(ref ev);
        //    base.OnSDLEvent(ref ev);
        //}

        private void WindowOnClientSizeChanged(object sender, EventArgs e)
        {
            int width = Window.ClientBounds.Width;
            int height = Window.ClientBounds.Height;

            if (!IsWindowMaximized())
            {
                ProfileManager.Current.WindowClientBounds = new Point(width, height);
            }

            SetWindowSize(width, height);

            var viewport = UIManager.GetGump<WorldViewportGump>();

            if (viewport != null && ProfileManager.Current.GameWindowFullSize)
            {
                viewport.ResizeGameWindow(new Point(width, height));
                viewport.X = -5;
                viewport.Y = -5;
            }
        }

        private unsafe int HandleSDLEvent(IntPtr userdata, IntPtr ptr)
        {
            SDL_Event* e = (SDL_Event*) ptr;

            if (Plugin.ProcessWndProc(e) != 0)
            {
                if (e->type == SDL_EventType.SDL_MOUSEMOTION)
                {
                    if (UIManager.GameCursor != null)
                    {
                        UIManager.GameCursor.AllowDrawSDLCursor = false;
                    }
                }
                return 0;
            }

            switch (e->type)
            {
                case SDL.SDL_EventType.SDL_AUDIODEVICEADDED:
                    Console.WriteLine("AUDIO ADDED: {0}", e->adevice.which);

                    break;

                case SDL.SDL_EventType.SDL_AUDIODEVICEREMOVED:
                    Console.WriteLine("AUDIO REMOVED: {0}", e->adevice.which);

                    break;


                case SDL.SDL_EventType.SDL_WINDOWEVENT:

                    switch (e->window.windowEvent)
                    {
                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                            Mouse.MouseInWindow = true;

                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                            Mouse.MouseInWindow = false;

                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                            Plugin.OnFocusGained();

                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                            Plugin.OnFocusLost();

                            break;
                    }

                    break;
                
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    
                    Keyboard.OnKeyDown(e->key);

                    if (Plugin.ProcessHotkeys((int) e->key.keysym.sym, (int) e->key.keysym.mod, true))
                    {
                        _ignoreNextTextInput = false;
                        UIManager.KeyboardFocusControl?.InvokeKeyDown(e->key.keysym.sym, e->key.keysym.mod);

                        _scene.OnKeyDown(e->key);
                    }
                    else
                        _ignoreNextTextInput = true;

                    break;

                case SDL.SDL_EventType.SDL_KEYUP:
                    
                    Keyboard.OnKeyUp(e->key);
                    UIManager.KeyboardFocusControl?.InvokeKeyUp(e->key.keysym.sym, e->key.keysym.mod);
                    _scene.OnKeyUp(e->key);
                    Plugin.ProcessHotkeys(0, 0, false);

                    if (e->key.keysym.sym == SDL_Keycode.SDLK_PRINTSCREEN)
                    {
                        // string path = Path.Combine(FileSystemHelper.CreateFolderIfNotExists(CUOEnviroment.ExecutablePath, "Data", "Client", "Screenshots"), $"screenshot_{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.png");
                        //
                        // Color[] colors = new Color[_graphicDeviceManager.PreferredBackBufferWidth * _graphicDeviceManager.PreferredBackBufferHeight];
                        // GraphicsDevice.GetBackBufferData(colors);
                        // using (Texture2D texture = new Texture2D(GraphicsDevice, _graphicDeviceManager.PreferredBackBufferWidth, _graphicDeviceManager.PreferredBackBufferHeight, false, SurfaceFormat.Color))
                        // {
                        //     texture.SetData(colors);
                        //
                        //     using (Stream stream = File.Create(path))
                        //     {
                        //         texture.SaveAsPng(stream, texture.Width, texture.Height);
                        //
                        //         GameActions.Print($"Screenshot stored in: {path}", 0x44, MessageType.System);
                        //     }
                        // }
                       
                    }

                    break;

                case SDL.SDL_EventType.SDL_TEXTINPUT:

                    if (_ignoreNextTextInput)
                        break;

                    string s = StringHelper.ReadUTF8(e->text.text);

                    if (!string.IsNullOrEmpty(s))
                    {
                        UIManager.KeyboardFocusControl?.InvokeTextInput(s);
                        _scene.OnTextInput(s);
                    }
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:

                    if (UIManager.GameCursor != null && !UIManager.GameCursor.AllowDrawSDLCursor)
                    {
                        UIManager.GameCursor.AllowDrawSDLCursor = true;
                        UIManager.GameCursor.Graphic = 0xFFFF;
                    }

                    Mouse.Update();

                    if (Mouse.IsDragging)
                    {
                        if (!_scene.OnMouseDragging())
                            UIManager.OnMouseDragging();
                    }

                    if (Mouse.IsDragging && !_dragStarted)
                    {
                        _dragStarted = true;
                    }

                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    Mouse.Update();
                    bool isup = e->wheel.y > 0;

                    Plugin.ProcessMouse(0, e->wheel.y);

                    if (!_scene.OnMouseWheel(isup))
                        UIManager.OnMouseWheel(isup);

                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    Mouse.Update();
                    bool isDown = e->type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN;

                    if (_dragStarted && !isDown)
                    {
                        _dragStarted = false;
                    }

                    SDL.SDL_MouseButtonEvent mouse = e->button;

                    switch ((uint) mouse.button)
                    {
                        case SDL_BUTTON_LEFT:

                            if (isDown)
                            {
                                Mouse.Begin();
                                Mouse.LButtonPressed = true;
                                Mouse.LDropPosition = Mouse.Position;
                                Mouse.CancelDoubleClick = false;
                                uint ticks = Time.Ticks;

                                if (Mouse.LastLeftButtonClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK >= ticks)
                                {
                                    Mouse.LastLeftButtonClickTime = 0;
                                 
                                    bool res = _scene.OnLeftMouseDoubleClick() || UIManager.OnLeftMouseDoubleClick();

                                    if (!res)
                                    {
                                        if (!_scene.OnLeftMouseDown())
                                            UIManager.OnLeftMouseButtonDown();
                                    }
                                    else
                                    {
                                        Mouse.LastLeftButtonClickTime = 0xFFFF_FFFF;
                                    }

                                    break;
                                }

                                if (!_scene.OnLeftMouseDown()) 
                                    UIManager.OnLeftMouseButtonDown();

                                Mouse.LastLeftButtonClickTime = Mouse.CancelDoubleClick ? 0 : ticks;
                            }
                            else
                            {
                                if (Mouse.LastLeftButtonClickTime != 0xFFFF_FFFF)
                                {
                                    if (!_scene.OnLeftMouseUp() || UIManager.LastControlMouseDown(MouseButtonType.Left) != null)
                                        UIManager.OnLeftMouseButtonUp();
                                }
                                Mouse.LButtonPressed = false;
                                Mouse.End();
                            }

                            break;

                        case SDL_BUTTON_MIDDLE:

                            if (isDown)
                            {
                                Mouse.Begin();
                                Mouse.MButtonPressed = true;
                                Mouse.MDropPosition = Mouse.Position;
                                Mouse.CancelDoubleClick = false;
                                uint ticks = Time.Ticks;

                                if (Mouse.LastMidButtonClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK >= ticks)
                                {
                                    Mouse.LastMidButtonClickTime = 0;

                                    bool res = _scene.OnMiddleMouseDoubleClick() || UIManager.OnMiddleMouseDoubleClick();

                                    if (!res)
                                    {
                                        if (!_scene.OnMiddleMouseDown())
                                            UIManager.OnMiddleMouseButtonDown();
                                    }
                                    else
                                    {
                                        Mouse.LastMidButtonClickTime = 0xFFFF_FFFF;
                                    }

                                    break;
                                }

                                Plugin.ProcessMouse(e->button.button, 0);

                                if (!_scene.OnMiddleMouseDown())
                                    UIManager.OnMiddleMouseButtonDown();

                                Mouse.LastMidButtonClickTime = Mouse.CancelDoubleClick ? 0 : ticks;
                            }
                            else
                            {
                                if (Mouse.LastMidButtonClickTime != 0xFFFF_FFFF)
                                {
                                    if (!_scene.OnMiddleMouseUp())
                                        UIManager.OnMiddleMouseButtonUp();
                                }

                                Mouse.MButtonPressed = false;
                                Mouse.End();
                            }

                            break;

                        case SDL_BUTTON_RIGHT:

                            if (isDown)
                            {
                                Mouse.Begin();
                                Mouse.RButtonPressed = true;
                                Mouse.RDropPosition = Mouse.Position;
                                Mouse.CancelDoubleClick = false;
                                uint ticks = Time.Ticks;

                                if (Mouse.LastRightButtonClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK >= ticks)
                                {
                                    Mouse.LastRightButtonClickTime = 0;

                                    bool res = _scene.OnRightMouseDoubleClick() || UIManager.OnRightMouseDoubleClick();

                                    if (!res)
                                    {
                                        if (!_scene.OnRightMouseDown())
                                            UIManager.OnRightMouseButtonDown();
                                    }
                                    else
                                    {
                                        Mouse.LastRightButtonClickTime = 0xFFFF_FFFF;
                                    }

                                    break;
                                }

                                if (!_scene.OnRightMouseDown())
                                    UIManager.OnRightMouseButtonDown();

                                Mouse.LastRightButtonClickTime = Mouse.CancelDoubleClick ? 0 : ticks;
                            }
                            else
                            {
                                if (Mouse.LastRightButtonClickTime != 0xFFFF_FFFF)
                                {
                                    if (!_scene.OnRightMouseUp())
                                        UIManager.OnRightMouseButtonUp();
                                }
                                Mouse.RButtonPressed = false;
                                Mouse.End();
                            }

                            break;

                        case SDL_BUTTON_X1:
                        case SDL_BUTTON_X2:
                            if (isDown)
                            {
                                Mouse.Begin();
                                Mouse.XButtonPressed = true;
                                Mouse.CancelDoubleClick = false;
                                Plugin.ProcessMouse(e->button.button, 0);
                                if (!_scene.OnExtraMouseDown(mouse.button - 1))
                                    UIManager.OnExtraMouseButtonDown(mouse.button - 1);

                                // TODO: doubleclick?
                            }
                            else
                            {
                                if (!_scene.OnExtraMouseUp(mouse.button - 1))
                                    UIManager.OnExtraMouseButtonUp(mouse.button - 1);

                                Mouse.XButtonPressed = false;
                                Mouse.End();
                            }

                            break;
                    }

                    break;
            }

            return 0;
        }

        private readonly UnityEngine.KeyCode[] _keyCodeEnumValues = (UnityEngine.KeyCode[]) Enum.GetValues(typeof(UnityEngine.KeyCode));
        private readonly Control[] controlsUnderFingers = new Control[5];
        private UnityEngine.Vector3 lastMousePosition;
        public SDL_Keymod KeymodOverride;

        private void MouseUpdate()
        {
            var oneOverScale = 1f / Batcher.scale;
            
            //Finger/mouse handling
            if (UnityEngine.Application.isMobilePlatform && UserPreferences.UseMouseOnMobile.CurrentValue == 0)
            {
                var fingers = Lean.Touch.LeanTouch.GetFingers(true, false);

                //Only process one finger that has not started over gui because using multiple fingers with UIManager
                //causes issues due to the assumption that there's only one pointer, such as on finger "stealing"
                //a dragged gump from another
                if (fingers.Count > 0)
                {
                    var finger = fingers[0];
                    
                    var leftMouseDown = finger.Down;
                    var leftMouseHeld = finger.Set;

                    var mousePositionPoint = ConvertUnityMousePosition(finger.ScreenPosition, oneOverScale);
                    Mouse.Position = mousePositionPoint;
                    Mouse.LButtonPressed = leftMouseDown || leftMouseHeld;
                    Mouse.RButtonPressed = false;
                    Mouse.IsDragging = Mouse.LButtonPressed || Mouse.RButtonPressed;
                    Mouse.RealPosition = Mouse.Position;
                }
            }
            else
            {
                var leftMouseDown = UnityEngine.Input.GetMouseButtonDown(0);
                var leftMouseHeld = UnityEngine.Input.GetMouseButton(0);
                var rightMouseDown = UnityEngine.Input.GetMouseButtonDown(1);
                var rightMouseHeld = UnityEngine.Input.GetMouseButton(1);
                var mousePosition = UnityEngine.Input.mousePosition;

                if (Lean.Touch.LeanTouch.PointOverGui(mousePosition))
                {
                    Mouse.Position.X = 0;
                    Mouse.Position.Y = 0;
                    leftMouseDown = false;
                    leftMouseHeld = false;
                    rightMouseDown = false;
                    rightMouseHeld = false;
                }
                
                var mousePositionPoint = ConvertUnityMousePosition(mousePosition, oneOverScale);
                Mouse.Position = mousePositionPoint;
                Mouse.LButtonPressed = leftMouseDown || leftMouseHeld;
                Mouse.RButtonPressed = rightMouseDown || rightMouseHeld;
                Mouse.IsDragging = Mouse.LButtonPressed || Mouse.RButtonPressed;
                Mouse.RealPosition = Mouse.Position;
            }
        }

        private void UnityInputUpdate()
        {
            var oneOverScale = 1f / Batcher.scale;
            
            //Finger/mouse handling
            if (UnityEngine.Application.isMobilePlatform && UserPreferences.UseMouseOnMobile.CurrentValue == 0)
            {
                var fingers = Lean.Touch.LeanTouch.GetFingers(true, false);
                
                //Detect two finger tap gesture for closing gumps
                for (int i = 0; i < fingers.Count && i < 5; i++)
                {
                    var finger = fingers[i];
                    if (finger.Age < 0.1f)
                    {
                        var mousePositionPoint = ConvertUnityMousePosition(finger.ScreenPosition, oneOverScale);
                        controlsUnderFingers[i] = UIManager.GetMouseOverControl(mousePositionPoint)?.RootParent;
                        if (controlsUnderFingers[i] != null)
                        {
                            for (int k = 0; k < i; k++)
                            {
                                if (controlsUnderFingers[k] == controlsUnderFingers[i])
                                {
                                    //Simulate right mouse down and up
                                    SimulateMouse(false, false, true, false, false, true);
                                    SimulateMouse(false, false, false, true, false, true);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        controlsUnderFingers[i] = null;
                    }
                }
                
                //Only process one finger that has not started over gui because using multiple fingers with UIManager
                //causes issues due to the assumption that there's only one pointer, such as on finger "stealing"
                //a dragged gump from another
                if (fingers.Count > 0)
                {
                    var finger = fingers[0];
                    var mouseMotion = finger.ScreenPosition != finger.LastScreenPosition;
                    SimulateMouse(finger.Down, finger.Up, false, false, mouseMotion, false);
                }
            }
            else
            {
                var leftMouseDown = UnityEngine.Input.GetMouseButtonDown(0);
                var leftMouseUp = UnityEngine.Input.GetMouseButtonUp(0);
                var rightMouseDown = UnityEngine.Input.GetMouseButtonDown(1);
                var rightMouseUp = UnityEngine.Input.GetMouseButtonUp(1);
                var mousePosition = UnityEngine.Input.mousePosition;
                var mouseMotion = mousePosition != lastMousePosition;
                lastMousePosition = mousePosition;
                
                if (Lean.Touch.LeanTouch.PointOverGui(mousePosition))
                {
                    Mouse.Position.X = 0;
                    Mouse.Position.Y = 0;
                    leftMouseDown = false;
                    leftMouseUp = false;
                    rightMouseDown = false;
                    rightMouseUp = false;
                }
                
                SimulateMouse(leftMouseDown, leftMouseUp, rightMouseDown, rightMouseUp, mouseMotion, false);
            }

            //Keyboard handling
            var keymod = KeymodOverride;
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt))
            {
                keymod |= SDL_Keymod.KMOD_LALT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt))
            {
                keymod |= SDL_Keymod.KMOD_RALT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift))
            {
                keymod |= SDL_Keymod.KMOD_LSHIFT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift))
            {
                keymod |= SDL_Keymod.KMOD_RSHIFT;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftControl))
            {
                keymod |= SDL_Keymod.KMOD_LCTRL;
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightControl))
            {
                keymod |= SDL_Keymod.KMOD_RCTRL;
            }
            foreach (var keyCode in _keyCodeEnumValues)
            {
                var key = new SDL_KeyboardEvent {keysym = new SDL_Keysym {sym = (SDL_Keycode) keyCode, mod = keymod}};
                if (UnityEngine.Input.GetKeyDown(keyCode))
                {
                    Keyboard.OnKeyDown(key);

                    if (Plugin.ProcessHotkeys((int) key.keysym.sym, (int) key.keysym.mod, true))
                    {
                        _ignoreNextTextInput = false;
                        UIManager.KeyboardFocusControl?.InvokeKeyDown(key.keysym.sym, key.keysym.mod);
                        _scene.OnKeyDown(key);
                    }
                    else
                        _ignoreNextTextInput = true;
                }
                if (UnityEngine.Input.GetKeyUp(keyCode))
                {
                    Keyboard.OnKeyUp(key);
                    UIManager.KeyboardFocusControl?.InvokeKeyUp(key.keysym.sym, key.keysym.mod);
                    _scene.OnKeyUp(key);
                    Plugin.ProcessHotkeys(0, 0, false);
                }
            }

            //Input text handling
            if (UnityEngine.Application.isMobilePlatform && TouchScreenKeyboard != null)
            {
                var text = TouchScreenKeyboard.text;
                
                if (_ignoreNextTextInput == false && TouchScreenKeyboard.status == UnityEngine.TouchScreenKeyboard.Status.Done)
                {
                    //Clear the text of TouchScreenKeyboard, otherwise it stays there and is re-evaluated every frame
                    TouchScreenKeyboard.text = string.Empty;
                    
                    //Set keyboard to null so we process its text only once when its status is set to Done
                    TouchScreenKeyboard = null;
                    
                    //Need to clear the existing text in textbox before "pasting" new text from TouchScreenKeyboard
                    if (UIManager.KeyboardFocusControl is StbTextBox stbTextBox)
                    {
                        stbTextBox.SetText(string.Empty);
                    }
                    
                    UIManager.KeyboardFocusControl?.InvokeTextInput(text);
                    _scene.OnTextInput(text);
                    
                    //When targeting SystemChat textbox, "auto-press" return key so that the text entered on the TouchScreenKeyboard is submitted right away
                    if (UIManager.KeyboardFocusControl != null && UIManager.KeyboardFocusControl == UIManager.SystemChat?.TextBoxControl)
                    {
                        //Handle different chat modes
                        HandleChatMode(text);
                        //"Press" return
                        UIManager.KeyboardFocusControl.InvokeKeyDown(SDL_Keycode.SDLK_RETURN, SDL_Keymod.KMOD_NONE);
                        //Revert chat mode to default
                        UIManager.SystemChat.Mode = ChatMode.Default;
                    }
                }
            }
            else
            {
                var text = UnityEngine.Input.inputString;
                //Backspace character should not be sent as text input
                text = text.Replace("\b", "");
                if (_ignoreNextTextInput == false && string.IsNullOrEmpty(text) == false)
                {
                    UIManager.KeyboardFocusControl?.InvokeTextInput(text);
                    _scene.OnTextInput(text);
                }
            }
        }

        private void HandleChatMode(string text)
        {
            if (text.Length > 0)
            {
                switch (text[0])
                {                  
                    case '/':
                        UIManager.SystemChat.Mode = ChatMode.Party;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(1));
                        break;
                    case '\\':
                        UIManager.SystemChat.Mode = ChatMode.Guild;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(1));
                        break;
                    case '|':
                        UIManager.SystemChat.Mode = ChatMode.Alliance;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(1));
                        break;
                    case '-':
                        UIManager.SystemChat.Mode = ChatMode.ClientCommand;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(1));
                        break;
                    case ',' when UOChatManager.ChatIsEnabled == CHAT_STATUS.ENABLED:
                        UIManager.SystemChat.Mode = ChatMode.UOChat;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(1));
                        break;
                    case ':' when text.Length > 1 && text[1] == ' ':
                        UIManager.SystemChat.Mode = ChatMode.Emote;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(2));
                        break;
                    case ';' when text.Length > 1 && text[1] == ' ':
                        UIManager.SystemChat.Mode = ChatMode.Whisper;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(2));
                        break;
                    case '!' when text.Length > 1 && text[1] == ' ':
                        UIManager.SystemChat.Mode = ChatMode.Yell;
                        //Textbox text has been cleared, set it again
                        UIManager.SystemChat.TextBoxControl.InvokeTextInput(text.Substring(2));
                        break;
                }
            }
        }

        private static Point ConvertUnityMousePosition(UnityEngine.Vector2 screenPosition, float oneOverScale)
        {
            var x = UnityEngine.Mathf.RoundToInt(screenPosition.x * oneOverScale);
            var y = UnityEngine.Mathf.RoundToInt((UnityEngine.Screen.height - screenPosition.y) * oneOverScale);
            return new Point(x, y);
        }

        private void SimulateMouse(bool leftMouseDown, bool leftMouseUp, bool rightMouseDown, bool rightMouseUp, bool mouseMotion, bool skipSceneInput)
        {
            if (_dragStarted && !Mouse.LButtonPressed)
            {
                _dragStarted = false;
            }
            
            if (leftMouseDown)
            {
                Mouse.LDropPosition = Mouse.Position;
                Mouse.CancelDoubleClick = false;
                uint ticks = Time.Ticks;
                if (Mouse.LastLeftButtonClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK >= ticks)
                {
                    Mouse.LastLeftButtonClickTime = 0;

                    var res = false;
                    if (skipSceneInput)
                    {
                        res = UIManager.OnLeftMouseDoubleClick();
                    }
                    else
                    {
                        res = _scene.OnLeftMouseDoubleClick() || UIManager.OnLeftMouseDoubleClick();
                    }

                    if (!res)
                    {
                        if (skipSceneInput || !_scene.OnLeftMouseDown())
                            UIManager.OnLeftMouseButtonDown();
                    }
                    else
                    {
                        Mouse.LastLeftButtonClickTime = 0xFFFF_FFFF;
                    }
                }
                else
                {
                    if (skipSceneInput || !_scene.OnLeftMouseDown())
                        UIManager.OnLeftMouseButtonDown();
                    Mouse.LastLeftButtonClickTime = Mouse.CancelDoubleClick ? 0 : ticks;
                }
            }
            else if (leftMouseUp)
            {
                if (Mouse.LastLeftButtonClickTime != 0xFFFF_FFFF)
                {
                    if (skipSceneInput || !_scene.OnLeftMouseUp() || UIManager.LastControlMouseDown(MouseButtonType.Left) != null)
                        UIManager.OnLeftMouseButtonUp();
                }

                Mouse.End();
            }

            if (rightMouseDown)
            {
                Mouse.RDropPosition = Mouse.Position;
                Mouse.CancelDoubleClick = false;
                uint ticks = Time.Ticks;

                if (Mouse.LastRightButtonClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK >= ticks)
                {
                    Mouse.LastRightButtonClickTime = 0;

                    var res = false;
                    if (skipSceneInput)
                    {
                        res = UIManager.OnRightMouseDoubleClick();
                    }
                    else
                    {
                        res = _scene.OnRightMouseDoubleClick() || UIManager.OnRightMouseDoubleClick();
                    }
                    
                    if (!res)
                    {
                        if (skipSceneInput || !_scene.OnRightMouseDown())
                            UIManager.OnRightMouseButtonDown();
                    }
                    else
                    {
                        Mouse.LastRightButtonClickTime = 0xFFFF_FFFF;
                    }
                }
                else
                {
                    if (skipSceneInput || !_scene.OnRightMouseDown())
                        UIManager.OnRightMouseButtonDown();
                    Mouse.LastRightButtonClickTime = Mouse.CancelDoubleClick ? 0 : ticks;
                }
            }
            else if (rightMouseUp)
            {
                if (Mouse.LastRightButtonClickTime != 0xFFFF_FFFF)
                {
                    if (skipSceneInput || !_scene.OnRightMouseUp())
                        UIManager.OnRightMouseButtonUp();
                }

                Mouse.End();
            }

            if (mouseMotion)
            {
                if (Mouse.IsDragging)
                {
                    if (skipSceneInput || !_scene.OnMouseDragging())
                        UIManager.OnMouseDragging();
                }

                if (Mouse.IsDragging && !_dragStarted)
                {
                    _dragStarted = true;
                }
            }
        }
    }
}
