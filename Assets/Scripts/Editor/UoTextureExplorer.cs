using UnityEditor;
using UnityEngine;

public class UoTextureExplorer : EditorWindow
{
    private int rangeMin;
    private int rangeMax;
    private Vector2 scrollPos;
    private bool skipNullTextures;

    private enum GraphicType
    {
        ArtGraphic,
        GumpGraphic
    }

    private GraphicType graphicTypeToDisplay = GraphicType.ArtGraphic;

    [MenuItem("Tools/Uo Texture Explorer")]
    public new static void Show()
    {
        GetWindow<UoTextureExplorer>();
    }

    private void OnGUI()
    {
        if (UoTextureExplorerHelper.NeedsLoading)
        {
            if (GUILayout.Button("Load Art"))
            {
                UoTextureExplorerHelper.LoadArt();
            }

            return;
        }

        if (GUILayout.Button("Unload Art"))
        {
            UoTextureExplorerHelper.UnloadArt();
            return;
        }

        if (GUILayout.Button("Make Atlas"))
        {
            UoTextureExplorerHelper.MakeAtlas(rangeMin, rangeMax);
        }

        if (GUILayout.Button("Trigger first texture"))
        {
            UoTextureExplorerHelper.TriggerFirstTexture();
        }

        graphicTypeToDisplay = (GraphicType) EditorGUILayout.EnumPopup(graphicTypeToDisplay);

        rangeMin = EditorGUILayout.IntField("Range Min", rangeMin);
        rangeMax = EditorGUILayout.IntField("Range Max", rangeMax);
        skipNullTextures = EditorGUILayout.Toggle("Skip null textures", skipNullTextures);

        int nullTextureCount = 0;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();
        var widthRemaining = position.width;
        Rect rect = new Rect(0,0,widthRemaining, 44 + 2);
        for (int i = rangeMin; i <= rangeMax; i++)
        {
            Microsoft.Xna.Framework.Graphics.Texture2D texture = null;
            if (graphicTypeToDisplay == GraphicType.ArtGraphic)
            {
                texture = UoTextureExplorerHelper.GetLandTexture((uint) i);
            }
            else if (graphicTypeToDisplay == GraphicType.GumpGraphic)
            {
                texture = UoTextureExplorerHelper.GetGumpTexture((ushort) i);
            }

            if (widthRemaining < 44)
            {
                GUILayout.Space(44 + 2);
                rect.y += 44 + 2;
                widthRemaining = rect.width;
            }

            if (texture != null)
            {
                GUI.DrawTexture(new Rect(rect.x + rect.width - widthRemaining, rect.y, 44, 44), texture.UnityTexture);
            }
            else
            {
                nullTextureCount++;
            }

            if (skipNullTextures == false || texture != null)
            {
                widthRemaining -= 44 + 2;
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("Null texture count", nullTextureCount.ToString());
    }
}
