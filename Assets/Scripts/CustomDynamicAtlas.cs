using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class CustomDynamicAtlas {

	public RenderTexture AtlasTexture { get; }
	public readonly Dictionary<int, Rect> TextureToRectDictionary;
	private readonly ColumnRectPack rectsPack;

	private int height;

	public CustomDynamicAtlas(int width, int height, string name)
	{
		this.height = height;
		AtlasTexture = new RenderTexture(width, height, 0) {name = name};
		AtlasTexture.Create();
		rectsPack = new ColumnRectPack(width, height);
		TextureToRectDictionary = new Dictionary<int, Rect>();
	}

	public void Insert(IEnumerable<Texture> textures) {
		var previousRt = RenderTexture.active;
		RenderTexture.active = AtlasTexture;
		GL.LoadPixelMatrix( 0, AtlasTexture.width, AtlasTexture.height, 0 );

		foreach (var source in textures)
		{
			var hashCode = source.GetHashCode();
			if (TextureToRectDictionary.ContainsKey(hashCode))
				continue;

			var newRect = rectsPack.Insert(source.width, source.height);
			if (newRect.height == 0)
				continue;

			var rect = new Rect(newRect.x, newRect.y, newRect.width, newRect.height);
			//Y position needs to be flipped to match unity uv origin
			var flippedRect = new Rect(newRect.x, height - newRect.y - newRect.height, newRect.width, newRect.height);
			TextureToRectDictionary.Add(hashCode, flippedRect);
			Graphics.DrawTexture(rect, source);
		}

		RenderTexture.active = previousRt;
	}

	public bool Insert(Texture source) {
		var hashCode = source.GetHashCode();
		if (TextureToRectDictionary.ContainsKey(hashCode))
			return false;

		var newRect = rectsPack.Insert(source.width, source.height);

		if (newRect.height == 0)
			return false;

		var rect = new Rect(newRect.x, newRect.y, newRect.width, newRect.height);
		//Y position needs to be flipped to match unity uv origin
		var flippedRect = new Rect(newRect.x, height - newRect.y - newRect.height, newRect.width, newRect.height);
		TextureToRectDictionary.Add(hashCode, flippedRect);

		var previousRt = RenderTexture.active;
		RenderTexture.active = AtlasTexture;
		GL.LoadPixelMatrix( 0, AtlasTexture.width, AtlasTexture.height, 0 );
		Graphics.DrawTexture(rect, source);
		RenderTexture.active = previousRt;

		return true;
	}

	public void Save()
	{
		var directoryPath = Path.Combine(Application.persistentDataPath, "DynamicAtlas");
		Directory.CreateDirectory(directoryPath);
		var path = Path.Combine(directoryPath, AtlasTexture.name + ".png");
		var tex = new Texture2D(AtlasTexture.width, AtlasTexture.height);
		var previousRt = RenderTexture.active;
		RenderTexture.active = AtlasTexture;
		tex.ReadPixels(new Rect(0, 0, AtlasTexture.width, AtlasTexture.height), 0, 0);
		tex.Apply();
		File.WriteAllBytes(path, tex.EncodeToPNG());
		RenderTexture.active = previousRt;
	}
}