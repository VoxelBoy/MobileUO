#if UNITY_EDITOR
using System.Collections.Generic;
using ClassicUO;
using ClassicUO.Configuration;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;
using Newtonsoft.Json;
using UnityEngine;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

public static class UoTextureExplorerHelper
{
    public static bool NeedsLoading => loaded == false;
    private static bool loaded;
    public static void LoadArt()
    {
        ConsoleRedirect.Redirect();
        Log.Start( LogTypes.All );
        Settings.GlobalSettings = JsonConvert.DeserializeObject<Settings>(Resources.Load<TextAsset>("settings").text);
        Client.Game = new GameController();
        //Calling the getter to trigger the creation of GraphicsDevice
        var graphicsDevice = Client.Game.GraphicsDevice;
        ArtLoader.Instance.Load().Wait();
        loaded = true;
    }

    public static void UnloadArt()
    {
        Client.Game.Dispose();
    }

    public static void TriggerFirstTexture()
    {
        ArtLoader.Instance.CleanResources();
        ArtLoader.Instance.GetLandTexture(0);
    }

    public static void CreateLandTileTextureAtlas()
    {
        if (loaded == false)
        {
            LoadArt();
        }

        //Report valid entry ranges
        Vector2Int range = default;
        int runningTotal = 0;
        int numberOfDuplicates = 0;
        HashSet<UOFileIndex> seenIndexes = new HashSet<UOFileIndex>();
        List<int> duplicateIndexes = new List<int>();
        for (int i = 0; i < 65536; i++)
        {
            var graphic = (ushort) i;
            //Value used from ArtLoader._graphicMask
            graphic &= 0x3FFF;
            ref readonly var entry = ref ArtLoader.Instance.GetValidRefEntry(graphic);

            if (entry.Length == 0)
            {
                if (range.y >= range.x)
                {
                    Debug.Log($"Range {range.x}-{range.y}");
                    runningTotal += (range.y - range.x) + 1;
                }

                range.x = i + 1;
            }
            else
            {
                range.y = i;
                if (seenIndexes.Add(entry) == false)
                {
                    numberOfDuplicates++;
                    duplicateIndexes.Add(i);
                }
                else
                {

                }
            }
        }

        Debug.Log($"Total number of valid ushort keys {runningTotal}");
        Debug.Log($"Number of duplicates {numberOfDuplicates}");
        Debug.Log($"Duplicate indexes range from {duplicateIndexes[0]} to {duplicateIndexes[duplicateIndexes.Count - 1]}");
        Debug.Log($"Number of unique tiles {runningTotal - numberOfDuplicates}");
    }

    public static Texture2D GetLandTexture(uint g)
    {
        var uoTexture = ArtLoader.Instance.GetLandTexture(g);
        return uoTexture.Contains(0, 0, false) ? uoTexture : null;
    }

    public static void MakeAtlas(int rangeMin, int rangeMax)
    {
        Dictionary<uint, Texture2D> textures = new Dictionary<uint, Texture2D>(rangeMax - rangeMin + 1);
        HashSet<UOFileIndex> seenIndexes = new HashSet<UOFileIndex>();
        HashSet<Hash128> seenTextureIndexes = new HashSet<Hash128>();
        for (int i = rangeMin; i <= rangeMax; i++)
        {
            var graphic = (ushort) i;
            //Value used from ArtLoader._graphicMask
            graphic &= 0x3FFF;
            ref readonly var entry = ref ArtLoader.Instance.GetValidRefEntry(graphic);
            if (entry.Length != 0 && seenIndexes.Add(entry))
            {
                var texture = GetLandTexture((uint) i);
                if (texture != null && seenTextureIndexes.Add(texture.UnityTexture.imageContentsHash))
                {
                    textures.Add((uint) i, texture);
                }
            }
        }

        var totalPixels = textures.Count * 44 * 44;
        int[] atlasSizes = {256, 512, 1024, 2048};
        int atlasSizeIndex = 0;
        int atlasSize = 0;
        while (atlasSize * atlasSize < totalPixels && atlasSizeIndex < atlasSizes.Length)
        {
            atlasSize = atlasSizes[atlasSizeIndex++];
        }

        if (atlasSize * atlasSize < totalPixels)
        {
            Debug.LogWarning($"Can't fit all textures onto atlas. Total pixels {totalPixels} vs atlas pixels {atlasSize * atlasSize}");
        }

        CustomDynamicAtlas atlas = new CustomDynamicAtlas(atlasSize, atlasSize, "customatlas");
        foreach (var keyValuePair in textures)
        {
            atlas.Insert(keyValuePair.Value.UnityTexture);
        }

        atlas.Save();
    }
}
#endif