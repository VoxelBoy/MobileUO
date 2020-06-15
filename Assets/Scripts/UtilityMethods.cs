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

    public static void SetHeightBasedOnChildren(RectTransform rectTransform)
    {
        var totalHeight = GetTotalHeightOfChildren(rectTransform);
        var size = rectTransform.sizeDelta;
        size.y = totalHeight;
        rectTransform.sizeDelta = size;
    }
}