using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UtilityMethods
{
    public static float GetTotalHeightOfChildren(RectTransform rectTransform)
    {
        var totalHeight = 0f;

        foreach (RectTransform childRectT in rectTransform)
        {
            totalHeight += childRectT.sizeDelta.y;
        }

        return totalHeight;
    }

    public static bool EssentialUoFilesExist(List<string> files)
    {
        return files.Any(x =>
        {
            var fileNameLowerCase = x.ToLowerInvariant();
            return fileNameLowerCase.Contains("anim.mul") || fileNameLowerCase.Contains("animationframe1.uop");
        });
    }
}