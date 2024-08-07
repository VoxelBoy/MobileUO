using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using SDL2;
using StbTextEditSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.UI.Controls
{
    class SelectableReadOnlyBox : StbTextBox
    {
        public SelectableReadOnlyBox(byte font, int max_char_count = -1, int maxWidth = 0, bool isunicode = true, FontStyle style = FontStyle.None, ushort hue = 0, TEXT_ALIGN_TYPE align = 0) : base(font, max_char_count, maxWidth, isunicode, style, hue, align)
        {
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            ControlKeys? stb_key = null;
            bool update_caret = false;

            switch (key)
            {
                case SDL.SDL_Keycode.SDLK_a when Keyboard.Ctrl:
                    SelectAll();
                    break;
                case SDL.SDL_Keycode.SDLK_ESCAPE:
                    SelectionStart = 0;
                    SelectionEnd = 0;
                    break;

                case SDL.SDL_Keycode.SDLK_INSERT when IsEditable:
                    stb_key = ControlKeys.InsertMode;
                    break;
                case SDL.SDL_Keycode.SDLK_c when Keyboard.Ctrl:
                    int selectStart = Math.Min(Stb.SelectStart, Stb.SelectEnd);
                    int selectEnd = Math.Max(Stb.SelectStart, Stb.SelectEnd);

                    if (selectStart < selectEnd && selectStart >= 0 && selectEnd - selectStart <= Text.Length)
                    {
                        SDL.SDL_SetClipboardText(Text.Substring(selectStart, selectEnd - selectStart));
                    }

                    break;
                case SDL.SDL_Keycode.SDLK_LEFT:
                    if (Keyboard.Ctrl && Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.WordLeft;
                    }
                    else if (Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.Left;
                    }
                    else if (Keyboard.Ctrl)
                    {
                        stb_key = ControlKeys.WordLeft;
                    }
                    else
                    {
                        stb_key = ControlKeys.Left;
                    }

                    update_caret = true;
                    break;
                case SDL.SDL_Keycode.SDLK_RIGHT:
                    if (Keyboard.Ctrl && Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.WordRight;
                    }
                    else if (Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.Right;
                    }
                    else if (Keyboard.Ctrl)
                    {
                        stb_key = ControlKeys.WordRight;
                    }
                    else
                    {
                        stb_key = ControlKeys.Right;
                    }
                    update_caret = true;
                    break;
                case SDL.SDL_Keycode.SDLK_HOME:
                    if (Keyboard.Ctrl && Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.TextStart;
                    }
                    else if (Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.LineStart;
                    }
                    else if (Keyboard.Ctrl)
                    {
                        stb_key = ControlKeys.TextStart;
                    }
                    else
                    {
                        stb_key = ControlKeys.LineStart;
                    }
                    update_caret = true;
                    break;
                case SDL.SDL_Keycode.SDLK_END:
                    if (Keyboard.Ctrl && Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.TextEnd;
                    }
                    else if (Keyboard.Shift)
                    {
                        stb_key = ControlKeys.Shift | ControlKeys.LineEnd;
                    }
                    else if (Keyboard.Ctrl)
                    {
                        stb_key = ControlKeys.TextEnd;
                    }
                    else
                    {
                        stb_key = ControlKeys.LineEnd;
                    }
                    update_caret = true;
                    break;
            }

            if (stb_key != null)
            {
                Stb.Key(stb_key.Value);
            }

            if (update_caret)
            {
                UpdateCaretScreenPosition();
            }
        }
    }
}
