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
using System.Linq;
using System.Text;

using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;

using Assistant;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using SDL2;

namespace ClassicUO.Game.UI.Controls
{
    internal class AssistHotkeyBox : Control
    {
        private readonly NiceButton _buttonSave, _buttonClear;
        private readonly Checkbox _PassToUO;
        private readonly HoveredLabel _label;
        private readonly GumpPicTiled _pic;

        private bool _actived;

        public AssistHotkeyBox(int x, int y, int width, int height, byte font, ushort hue)
        {
            CanMove = false;
            AcceptMouseInput = true;
            AcceptKeyboardInput = true;


            Width = width;
            Height = height;

            Add(_pic = new GumpPicTiled(1, 0, width, 20, 0xBBC) { AcceptKeyboardInput = true, Hue = 666 });
            Add(_label = new HoveredLabel(string.Empty, false, 0x0025, 0x0025, 0x0025, width - 4, font, FontStyle.Cropped, TEXT_ALIGN_TYPE.TS_CENTER)
            {
                X = 1,
                Y = 5
            });
            _pic.MouseExit += AssistHotkeyBox_FocusLost;
            _label.MouseExit += AssistHotkeyBox_FocusLost;
            _pic.MouseEnter += AssistHotkeyBox_FocusEnter;
            _label.MouseEnter += AssistHotkeyBox_FocusEnter;
            Add(_PassToUO = new Checkbox(210, 211, "Pass to CUO", font, hue, true) { X = 0, Y = _label.Height + 10 });

            Add(_buttonClear = new NiceButton(10, _PassToUO.Y + _PassToUO.Height + 10, (width >> 2) + (width >> 3), 20, ButtonAction.Activate, "Clear")
            {
                IsSelectable = false,
                ButtonParameter = (int)ButtonState.Clear
            });

            Add(_buttonSave = new NiceButton(_buttonClear.X + _buttonClear.Width + (width >> 3), _buttonClear.Y, (width >> 2) + (width >> 3), 20, ButtonAction.Activate, "Save")
            {
                IsSelectable = false,
                ButtonParameter = (int)ButtonState.Save
            });

            //NOTE: Added Add Button
            Add(new NiceButton(10, _buttonClear.Y + _buttonClear.Height + 10, width - 20, 20, ButtonAction.Activate, "Add Button")
            {
                IsSelectable = false,
                ButtonParameter = (int) ButtonState.AddMacroButton
            });

            Height += 20;

            X = x;
            Y = y;
            WantUpdateSize = false;
            IsActive = false;
        }

        private void AssistHotkeyBox_FocusLost(object sender, EventArgs e)
        {
            IsActive = false;
        }

        private void AssistHotkeyBox_FocusEnter(object sender, EventArgs e)
        {
            IsActive = true;
        }

        public SDL.SDL_Keycode Key { get; private set; }
        public SDL.SDL_Keymod Mod { get; private set; }
        public MacroAction PassToCUO => (_PassToUO?.IsChecked ?? false) ? MacroAction.PassToUO : MacroAction.None;

        public bool IsActive
        {
            get => _actived;
            set
            {
                if (value != _actived)
                {
                    _actived = value;
                    if (_actived)
                    {
                        _pic.Hue = 0;
                        SetKeyboardFocus();
                    }
                    else
                    {
                        _pic.Hue = 666;
                    }
                }
            }
        }

        public event EventHandler HotkeyChanged, HotkeyCleared, AddButton;


        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (IsActive)
            {
                _KeyMod = mod;
                if(key != SDL.SDL_Keycode.SDLK_UNKNOWN)
                    SetKey(key, mod);
            }
        }

        protected override void OnKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (IsActive)
            {
                _KeyMod = SDL.SDL_Keymod.KMOD_NONE;
            }
        }

        private SDL.SDL_Keymod _KeyMod { get; set; }
        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            base.OnMouseUp(x, y, button);
            if (IsActive && (int)button >= 2 && (int)button <= 6)
            {
                SetKey((SDL.SDL_Keycode)button, _KeyMod);
            }
        }

        protected override void OnMouseWheel(MouseEventType delta)
        {
            base.OnMouseWheel(delta);
            if (IsActive && (delta == MouseEventType.WheelScrollUp || delta == MouseEventType.WheelScrollDown))
            {
                SetKey((SDL.SDL_Keycode)((int)delta + 0xFD), _KeyMod);
            }
        }

        public void SetKey(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            mod = (SDL.SDL_Keymod)((ushort)mod & 0x3C3);
            if (key == SDL.SDL_Keycode.SDLK_UNKNOWN && mod == SDL.SDL_Keymod.KMOD_NONE)
            {
                Key = key;
                Mod = mod;
                _label.Text = string.Empty;
            }
            else
            {
                string newvalue = TryGetKey(key, mod);

                if (!string.IsNullOrEmpty(newvalue) && key != SDL.SDL_Keycode.SDLK_UNKNOWN)
                {
                    Key = key;
                    Mod = mod;
                    _label.Text = newvalue;
                }
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            switch ((ButtonState)buttonID)
            {
                case ButtonState.Save:
                    HotkeyChanged.Raise(this);

                    break;

                case ButtonState.Clear:
                    _label.Text = string.Empty;

                    HotkeyCleared.Raise(this);

                    Key = SDL.SDL_Keycode.SDLK_UNKNOWN;
                    Mod = SDL.SDL_Keymod.KMOD_NONE;

                    break;
                case ButtonState.AddMacroButton:
                    AddButton.Raise(this);
                    break;
            }

            IsActive = false;
        }

        private enum ButtonState
        {
            Save=111,
            Clear=222,
            AddMacroButton=333
        }

        public static string TryGetKey(SDL.SDL_Keycode key, SDL.SDL_Keymod mod = SDL.SDL_Keymod.KMOD_NONE)
        {
            if (XmlFileParser.SDLkeyToVK.TryGetValue(key, out (uint vkey, string name) value))
            {
                StringBuilder sb = new StringBuilder();

                bool isshift = (mod & SDL.SDL_Keymod.KMOD_SHIFT) != SDL.SDL_Keymod.KMOD_NONE;
                bool isctrl = (mod & SDL.SDL_Keymod.KMOD_CTRL) != SDL.SDL_Keymod.KMOD_NONE;
                bool isalt = (mod & SDL.SDL_Keymod.KMOD_ALT) != SDL.SDL_Keymod.KMOD_NONE;


                if (isshift)
                    sb.Append("Shift ");

                if (isctrl)
                    sb.Append("Ctrl ");

                if (isalt)
                    sb.Append("Alt ");


                sb.Append(value.name);

                return sb.ToString();
            }

            return string.Empty;
        }
    }
}
