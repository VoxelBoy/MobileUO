using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public class ColumnRectPack
    {
        public struct Rect
        {
            public int x;
            public int y;
            public int width;
            public int height;

            public Rect(int x, int y, int width, int height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }

            public bool Equals(Rect rect)
            {
                return x == rect.x && y == rect.y && width == rect.width && height == rect.height;
            }
        }

        private readonly Dictionary<int, List<Rect>> columns = new Dictionary<int, List<Rect>>();

        private readonly int binWidth;
        private readonly int binHeight;
        private readonly int roundEveryX;
        private readonly float roundEveryXFloat;

        private int remainingWidth;

        public ColumnRectPack(int width, int height, int roundEveryX = 5)
        {
            binWidth = width;
            binHeight = height;
            remainingWidth = binWidth;
            this.roundEveryX = roundEveryX;
            roundEveryXFloat = roundEveryX;
        }

        public Rect Insert(int width, int height)
        {
            Rect rectToReturn = default;
            var roundedWidth = Mathf.RoundToInt(width / roundEveryXFloat) * roundEveryX;
            if (columns.TryGetValue(roundedWidth, out var rects))
            {
                for (int i = 0; i < rects.Count; i++)
                {
                    var rect = rects[i];
                    if (rect.height >= height)
                    {
                        rectToReturn = new Rect(rect.x, rect.y, width, height);
                        rect.y += height;
                        rect.height -= height;
                        rects[i] = rect;
                        break;
                    }
                }

                if (rectToReturn.Equals(default))
                {
                    if (remainingWidth >= roundedWidth)
                    {
                        rects.Add(new Rect(binWidth - remainingWidth, 0, roundedWidth, binHeight));
                        remainingWidth -= roundedWidth;
                    }
                }
            }
            else
            {
                if (remainingWidth >= roundedWidth)
                {
                    columns.Add(roundedWidth, new List<Rect>{new Rect(binWidth - remainingWidth, 0, roundedWidth, binHeight)});
                    remainingWidth -= roundedWidth;
                }
                else
                {
                    //There's no column matching roundedWidth and no space left to add a new column of this size
                    //Try to find a rect in another column that's >= width
                    var sortedKeys = columns.Keys.ToList();
                    sortedKeys.Sort();
                    var firstValidKeyIndex = sortedKeys.FindIndex(x => x >= width);
                    for (int k = firstValidKeyIndex; k < sortedKeys.Count; k++)
                    {
                        rects = columns[sortedKeys[k]];
                        for (int i = 0; i < rects.Count; i++)
                        {
                            var rect = rects[i];
                            if (rect.height >= height)
                            {
                                rectToReturn = new Rect(rect.x, rect.y, width, height);
                                rect.y += height;
                                rect.height -= height;
                                rects[i] = rect;
                                break;
                            }
                        }
                    }
                }
            }

            return rectToReturn;
        }
    }
}