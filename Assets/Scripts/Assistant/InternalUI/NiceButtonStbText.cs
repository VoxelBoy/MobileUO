using System.Collections.Generic;
using System.Linq;
using System;

using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Game.Managers;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Controls
{
    internal class NiceButtonStbText : HitBox
    {
        private readonly ButtonAction _action;
        private readonly int _groupnumber;
        private bool _isSelected;
        private byte _SelectedArea = 0;
        internal Label TextLabel { get; }
        internal StbTextBox[] TextBoxes { get; }
        internal AssistCheckbox Checkbox { get; }

        public NiceButtonStbText(int x, int y, int h, ButtonAction action, byte font, int groupnumber, TEXT_ALIGN_TYPE align, int[] width, byte labelentrynum = 0, bool hascheckbox = false, params string[] text) : base(x, y, width.Sum() + (width.Length * 3), h)
        {
            if (width.Length < text.Length)
                throw new System.Exception("the width and numberonly array must contain the same number of elements of text parameter");
            else if(text.Length < 1)
                throw new System.Exception("the text parameter cannot be empty!");
            else if(labelentrynum >= text.Length)
                throw new System.Exception("the labelentrynum must be lower than the number of text element!");
            _action = action;
            TextBoxes = new StbTextBox[text.Length - 1];
            int subx = 0, xtra = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                if (i > 0)
                {
                    if (i == 1)
                        subx -= xtra;
                    subx += width[i - 1] + 3;
                }
                else if(hascheckbox)
                {
                    Add(Checkbox = new AssistCheckbox(0x00D2, 0x00D3, "", font, ScriptTextBox.GRAY_HUE, true) { Priority = ClickPriority.High });
                    subx = xtra = Checkbox.Width;
                    if (width[0] - subx < 0)
                        throw new System.Exception("the primary width must be greater than zero, but the checkbox is eating all the space available");
                }
                if(i == labelentrynum)
                {
                    Add(TextLabel = new Label(text[i], true, 999, width[i] - (i == 0 ? xtra : 0), 0xFF, FontStyle.BlackBorder | FontStyle.Cropped, align) { X = subx });
                    TextLabel.Y = (h - TextLabel.Height) >> 1;
                }
                else
                {
                    Add(TextBoxes[(i > labelentrynum ? i - 1 : i)] = new StbTextBox(font, -1, width[i] - (i == 0 ? subx : 0), true, FontStyle.BlackBorder | FontStyle.Cropped, 999, align) { Width = width[i], Text = text[i], X = subx, Priority = ClickPriority.High });
                    if (i == 0)
                    {
                        TextBoxes[0].Width -= xtra;
                    }
                }
            }
            for(int i = 0; i < TextBoxes.Length; ++i)
            {
                TextBoxes[i].Y = TextLabel.Y;
                TextBoxes[i].Height = TextLabel.Height;
                TextBoxes[i].FocusEnter += NiceButtonStbText_FocusEnter;
                TextBoxes[i].FocusLost += NiceButtonStbText_FocusLost;
                //TextBoxes[i].KeyDown += NiceButtonStbText_KeyDown;
            }
            _groupnumber = groupnumber;
        }

        /*private void NiceButtonStbText_KeyDown(object sender, KeyboardEventArgs e)
        {
            if(e.Key == SDL2.SDL.SDL_Keycode.SDLK_RETURN || e.Key == SDL2.SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                foreach(StbTextBox stb in TextBoxes)
                {
                    if(stb.IsFocused)
                    {
                        stb.OnFocusLost();
                        stb.OnFocusEnter();
                        return;
                    }
                }
            }
        }*/

        private void NiceButtonStbText_FocusLost(object sender, System.EventArgs e)
        {
            Parent?.OnFocusLost();
        }

        private void NiceButtonStbText_FocusEnter(object sender, System.EventArgs e)
        {
            Parent?.OnFocusEnter();
        }

        public int ButtonParameter { get; set; }

        public bool IsSelectable { get; set; } = true;

        public bool IsSelected
        {
            get => _isSelected && IsSelectable;
            set
            {
                if (!IsSelectable)
                    return;

                _isSelected = value;

                if (value)
                {
                    Control p = Parent;

                    if (p == null)
                        return;

                    IEnumerable<NiceButtonStbText> list;

                    if (p is ScrollAreaItem)
                    {
                        p = p.Parent;
                        list = p.FindControls<ScrollAreaItem>().SelectMany(s => s.Children.OfType<NiceButtonStbText>());
                    }
                    else
                        list = p.FindControls<NiceButtonStbText>();

                    foreach (var b in list)
                        if (b != this && b._groupnumber == _groupnumber)
                            b.IsSelected = false;
                }
            }
        }

        internal static NiceButtonStbText GetSelected(Control p, int group)
        {
            IEnumerable<NiceButtonStbText> list = p is ScrollArea ? p.FindControls<ScrollAreaItem>().SelectMany(s => s.Children.OfType<NiceButtonStbText>()) : p.FindControls<NiceButtonStbText>();

            foreach (var b in list)
                if (b._groupnumber == group && b.IsSelected)
                    return b;

            return null;
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button == MouseButtonType.Left)
            {
                IsSelected = true;
                bool found = false;
                for(int i = TextBoxes.Length - 1; i >= 0 && !found; --i)
                {
                    if (x >= TextBoxes[i].X && x <= TextBoxes[i].X + TextBoxes[i].Width)
                    {
                        found = true;
                        _SelectedArea = (byte)(i + 1);
                    }
                }
                if(!found)
                    _SelectedArea = 0;

                if (_action == ButtonAction.SwitchPage)
                    ChangePage(ButtonParameter);
                else
                    OnButtonClick(ButtonParameter);
            }
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (IsSelected)
            {
                ResetHueVector();
                ShaderHueTranslator.GetHueVector(ref _hueVector, 0, false, Alpha);
                if (_SelectedArea > 0)
                    batcher.Draw2D(_texture, x + TextBoxes[_SelectedArea - 1].X, y, 0, 0, TextBoxes[_SelectedArea - 1].Width, Height, ref _hueVector);
                else
                    batcher.Draw2D(_texture, x + TextLabel.X, y, 0, 0, TextLabel.Width, Height, ref _hueVector);
            }

            return base.Draw(batcher, x, y);
        }
    }
}
