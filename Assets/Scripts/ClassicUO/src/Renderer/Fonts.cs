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

namespace ClassicUO.Renderer
{
    internal static class Fonts
    {
        public static SpriteFont Regular { get; private set; }
        public static SpriteFont Bold { get; private set; }
        public static SpriteFont Map1 { get; private set; }
        public static SpriteFont Map2 { get; private set; }
        public static SpriteFont Map3 { get; private set; }
        public static SpriteFont Map4 { get; private set; }
        public static SpriteFont Map5 { get; private set; }
        public static SpriteFont Map6 { get; private set; }

        static Fonts()
        {
            Regular = SpriteFont.Create("regular_font");
            Bold = SpriteFont.Create("bold_font");

            Map1 = SpriteFont.Create("map1_font");
            Map2 = SpriteFont.Create("map2_font");
            Map3 = SpriteFont.Create("map3_font");
            Map4 = SpriteFont.Create("map4_font");
            Map5 = SpriteFont.Create("map5_font");
            Map6 = SpriteFont.Create("map6_font");
        }
    }
}