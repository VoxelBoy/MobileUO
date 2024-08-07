using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UOScript;
using Assistant;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.UI.Controls
{
    internal class ScriptTextBox : StbTextBox
    {
        static StringBuilder _ssb = new StringBuilder();
        internal const ushort GRAY_HUE = 0x384;
        internal const ushort GREEN_HUE = 0x3F;
        internal const ushort RED_HUE = 0x23;
        internal const ushort BLUE_HUE = 0x5A;
        internal const ushort YELLOW_HUE = 0x90;
        static Dictionary<ushort, List<Rectangle2D>> HuedText = new Dictionary<ushort, List<Rectangle2D>>();

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            Rectangle scissor = ScissorStack.CalculateScissors(Matrix.Identity, x, y, Width, Height);

            if (ScissorStack.PushScissors(batcher.GraphicsDevice, scissor))
            {
                batcher.EnableScissorTest(true);

                DrawSelection(batcher, x, y);

                _rendererText.Draw(batcher, x, y);
                foreach (KeyValuePair<ushort, List<Rectangle2D>> kvp in HuedText)
                {
                    foreach(Rectangle2D r in kvp.Value)
                    {
                        _rendererText.Draw(batcher, x + r.X, y + r.Y, r.X, r.Y, r.Width, r.Height, kvp.Key);
                    }
                }
                DrawCaret(batcher, x, y);

                batcher.EnableScissorTest(false);
                ScissorStack.PopScissors(batcher.GraphicsDevice);
            }

            return true;
        }

        public ScriptTextBox(byte font, int width) : base(font, -1, width - 28, true, FontStyle.BlackBorder, 1153, TEXT_ALIGN_TYPE.TS_LEFT)
        {
            Height = 40;
            Width = width - 14;
            Multiline = true;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            _sfp.UpdateCoords(this);
            if(Interpreter.CmdArgs.Count > 0)
            {
                BuildContextMenu();
            }
        }

        private void BuildContextMenu()
        {
            int startidx = _sfp.CaretIdx, len = 0;
            if (startidx < 0 || startidx >= Text.Length)
                return;
            for (int i = startidx; i >= 0 && i < Text.Length; --i)
            {
                if (Text[i] == ' ' || Text[i] == '\n')
                {
                    startidx = i + 1;
                    break;
                }
                else if (i == 0)
                {
                    len++;
                    startidx = i;
                }
                else
                    ++len;
            }
            ContextMenu?.Dispose();
            ContextMenu = new ContextMenuControl();
            foreach(string s in Interpreter.CmdArgs)
            {
                ContextMenu.Add(new ContextMenuItemEntry(s, () => OnContextClicked(s, startidx, len), true));
            }

            ContextMenu?.Show();
        }

        /*private void Show()
        {
            UIManager.ShowContextMenu(null);

            if (_items.Count == 0)
                return;

            UIManager.ShowContextMenu
            (
                new ContextMenuShowMenu(_items)
                {
                    X = Mouse.Position.X + 5,
                    Y = Mouse.Position.Y - 20
                }
            );
        }*/

        private void OnContextClicked(string text, int start, int oldlen)
        {
            SetText(Text.Remove(start, oldlen).Insert(start, text));
            //CaretIndex += text.Length - oldlen;
        }

        private ScriptFileParser _sfp = new ScriptFileParser(new char[] { ' ', '(', ')', '\\', '/' }, new char[] { '/', '/' }, new char[] { '\'', '\'', '"', '"' });
        private class ScriptFileParser
        {
            private readonly char[] _delimiters, _comments, _quotes;
            private int x, y, w, h;
            private int _Size, _CaretIdx;
            internal int CaretIdx => _CaretIdx;
            private string _string;
            private MultilinesFontInfo _info;
            private RenderedText _r;
            private List<Rectangle2D> _rect = new List<Rectangle2D>();

            private bool _grab;
            private bool Grab
            {
                get { return _grab; }
                set
                {
                    if (_grab != value)
                    {
                        _grab = value;
                        if (_grab)
                        {
                            Point p = _r.GetCaretPosition(Pos);
                            x = p.X;
                            y = p.Y + 2;
                            h = _info.MaxHeight;
                            w = 0;
                        }
                        else
                        {
                            _ssb.Clear();
                            _rect.Clear();
                        }
                    }
                }
            }

            private int _pos;
            private int Pos
            {
                get
                {
                    return _pos;
                }
                set
                {
                    if (value != _pos)
                    {
                        if (value < _pos)
                        {
                            if (value != 0)
                                throw new Exception("backward movement not supported in ScriptFileParser");
                        }
                        else
                        {
                            for (int i = _pos; i < value && i < _Size; i++)
                            {
                                if(i == 0 && Grab)
                                {
                                    w += _r.GetCharWidth(_string[i]);
                                }
                                else if (i > 0)
                                {
                                    if (Grab)
                                    {
                                        if (i >= _info.CharStart + _info.CharCount)
                                        {
                                            /*p = _r.GetCaretPosition(i - 1);
                                            w = p.X + _r.GetCharWidth(_string[i - 1]);*/
                                            _rect.Add(new Rectangle2D(x, y, w, h));
                                            Point p = _r.GetCaretPosition(i);
                                            x = p.X;
                                            y = p.Y + 2;
                                            w = _r.GetCharWidth(_string[i]);
                                            var prev = _info;
                                            _info = _info.Next;
                                            h = _info?.MaxHeight ?? prev.MaxHeight;
                                        }
                                        else
                                            w += _r.GetCharWidth(_string[i]);
                                    }
                                }
                                else if (_string[i] == '\n')
                                {
                                    _info = _info.Next;
                                }
                            }
                        }
                        _pos = value;
                    }
                }
            }

            public ScriptFileParser(char[] delimiters, char[] comments, char[] quotes)
            {
                _delimiters = delimiters;
                _comments = comments;
                _quotes = quotes;
                _Size = 0;
                _string = "";
            }

            private bool IsDelimiter()
            {
                bool result = false;

                for (int i = 0; i < _delimiters.Length && !result; i++)
                    result = _string[Pos] == _delimiters[i];

                return result;
            }

            private void SkipToData()
            {
                while (Pos < _Size && ((IsDelimiter() && !IsComment()) || _string[Pos] == ' '))
                {
                    Pos++;
                }
            }

            private bool IsComment()
            {
                bool result = false;

                for (int i = 0; i < _comments.Length && !result; i++)
                {
                    result = _string[Pos] == _comments[i];

                    if (result && i + 1 < _comments.Length && _comments[i] == _comments[i + 1] && Pos + 1 < _Size)
                    {
                        result = _string[Pos] == _string[Pos + 1];
                        i++;
                    }
                }

                return result;
            }

            private void ObtainData()
            {
                while (Pos < _Size && _string[Pos] != '\n' && !IsComment())
                {
                    if (IsDelimiter())
                    {
                        break;
                    }
                    if (_string[Pos] != ' ')
                    {
                        Grab = true;
                        _ssb.Append(_string[Pos]);
                        if ((_CaretIdx == Pos || _CaretIdx - 1 == Pos) && _ssb.Length > 1 && (_string.Length == Pos + 1 || (Pos + 1 < _string.Length && (_string[Pos + 1] == ' ' || _string[Pos + 1] == '\n'))))//minimum 2 chars for autocomplete
                        {
                            //autocomplete code
                            if (_CaretIdx - 1 == Pos)
                                --_CaretIdx;
                            Interpreter.GetCmdArgs(_ssb.ToString());
                        }
                    }
                    else
                        break;
                    Pos++;
                }
                if (Grab)
                {
                    ushort col = Interpreter.ReferenceColor(_ssb.ToString().Trim('@', '!'));
                    if (col > 0)
                        AddHuedCoords(col);
                }
            }

            private void ObtainQuotedData(bool grab = true)
            {
                bool exit = false;

                for (int i = 0; i < _quotes.Length; i += 2)
                {
                    if (_string[Pos] == _quotes[i])
                    {
                        Grab = true;
                        char endQuote = _quotes[i + 1];
                        exit = true;
                        int pos = Pos + 1;
                        int start = pos;

                        while (pos < _Size && _string[pos] != '\n' && _string[pos] != endQuote)
                        {
                            if (_string[pos] == _quotes[i]) // another {
                            {
                                Pos = pos;
                                ObtainQuotedData(false); // skip
                                pos = Pos;
                            }
                            pos++;
                        }

                        Pos++;
                        int size = pos - start;

                        if (size > 0)
                        {
                            Pos = pos;

                            if (Pos < _Size && _string[Pos] == endQuote)
                            {
                                Pos++;
                                if (grab)
                                    AddHuedCoords(GREEN_HUE);
                            }
                        }

                        break;
                    }
                }
                if (!exit && grab)
                {
                    ObtainData();
                }

            }

            internal void UpdateCoords(ScriptTextBox str)
            {
                HuedText.Clear();
                str.ContextMenu?.Dispose();
                Interpreter.CmdArgs.Clear();
                _r = str._rendererText;
                _string = str.Text;
                _Size = _string.Length;
                _CaretIdx = Math.Max(0, str.CaretIndex - 1);//previous position before text input
                _info = str.GetInfo();
                Pos = 0;
                while (Pos < _Size)
                {
                    Grab = false;
                    SkipToData();
                    if (Pos >= _Size)
                        break;
                    if (_string[Pos] == '\n')
                    {
                        Pos++;
                        continue;
                    }
                    if (IsComment())
                    {
                        Grab = true;
                        while (Pos < _Size && _string[Pos] != '\n')
                        {
                            Pos++;
                        }
                        AddHuedCoords(GRAY_HUE);
                        Pos++;

                        continue;
                    }
                    ObtainQuotedData();
                }
            }

            private void AddHuedCoords(ushort hue)
            {
                if (!HuedText.TryGetValue(hue, out var l))
                    HuedText[hue] = l = new List<Rectangle2D>();
                _rect.Add(new Rectangle2D(x, y, w, h));
                l.AddRange(_rect);
            }
        }
    }
}
