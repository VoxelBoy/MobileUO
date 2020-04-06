using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ClassicUO;
using ClassicUO.Utility.Logging;
using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Scenes;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;

public class UnityMain : MonoBehaviour
{
	[SerializeField] private Texture2D hueTex1;
	[SerializeField] private Texture2D hueTex2;

	[SerializeField]
	private bool useDynamicAtlas;
	[SerializeField]
	public bool useGraphicsDrawTexture;
	[SerializeField]
	private bool forceEnterWorld;
	[SerializeField]
	private bool saveAllTextures;

	[SerializeField]
	private bool useImportedXnaHueTextures;

	[SerializeField]
	private bool scaleGameToFitScreen;
	public bool ScaleGameToFitScreen
	{
		get => scaleGameToFitScreen;
		set
		{
			scaleGameToFitScreen = value;
			ApplyScalingFactor();
			//Force update game viewport render texture
			if (Client.Game.Scene is GameScene gameScene)
			{
				gameScene.GetViewPort();
			}
		}
	}

	private ScaleSizes customScaleSize = ScaleSizes.Default;

	public ScaleSizes CustomScaleSize
	{
		get => customScaleSize;
		set
		{
			customScaleSize = value;
			ApplyScalingFactor();
			//Force update game viewport render texture
			if (Client.Game != null && Client.Game.Scene is GameScene gameScene)
			{
				gameScene.GetViewPort();
			}
		}
	}

	private static readonly int ScissorStateOverrideNameId = Shader.PropertyToID("ScissorStateOverride");

	private ScissorStateOverride scissorStateOverride = ScissorStateOverride.On;
	public ScissorStateOverride ScissorStateOverride
	{
		get => scissorStateOverride;
		set
		{
			scissorStateOverride = value;
			Shader.SetGlobalInt(ScissorStateOverrideNameId, (int) scissorStateOverride);
		}
	}

	[Header("Controls")]
	[SerializeField]
	private MobileJoystick movementJoystick;

	[SerializeField]
	private bool showMovementJoystickOnNonMobilePlatforms;

	private int lastScreenWidth;
	private int lastScreenHeight;

	private Texture2D generatedHueTexture1;
	private Texture2D generatedHueTexture2;

	void Start ()
    {
	    movementJoystick.gameObject.SetActive(false);
    }

	void Update ()
	{
		if (Client.Game == null)
			return;

		if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
		{
			lastScreenWidth = Screen.width;
			lastScreenHeight = Screen.height;
			//Force update ScaleGameToFitScreen
			ScaleGameToFitScreen = scaleGameToFitScreen;
		}

		if (forceEnterWorld && Client.Game.Scene is LoginScene)
		{
			ProfileManager.Load("fakeserver", "fakeaccount", "fakecharacter");
			World.Mobiles.Add(World.Player = new PlayerMobile(0));
			World.MapIndex = 0;
			World.Player.X = 1443;
			World.Player.Y = 1677;
			World.Player.Z = 0;
			World.Player.UpdateScreenPosition();
			World.Player.ProcessDelta();
			World.Mobiles.ProcessDelta();
			Client.Game.SetScene(new GameScene());
		}

		float deltaTime = UnityEngine.Time.deltaTime;
        if(deltaTime > 0.050f)
        {
            deltaTime = 0.050f;
        }

        if ((Application.isMobilePlatform || showMovementJoystickOnNonMobilePlatforms) && Client.Game.Scene is GameScene gameScene)
        {
	        gameScene.JoystickInput = new Microsoft.Xna.Framework.Vector2(movementJoystick.Position.x, -1 * movementJoystick.Position.y);
        }

        //NOTE: TODO: Use LeanTouch and StartedOverGui instead
        Client.Game.MouseOverGui = PointOverGui(Input.mousePosition);
        Client.Game.Tick(deltaTime);
	}

    private void OnPostRender()
    {
	    if (Client.Game == null)
		    return;

	    Client.Game.GraphicsDevice.Textures[1].UnityTexture = useImportedXnaHueTextures ? hueTex1 : generatedHueTexture1;
	    Client.Game.GraphicsDevice.Textures[2].UnityTexture = useImportedXnaHueTextures ? hueTex2 : generatedHueTexture2;

        GL.LoadPixelMatrix( 0, Screen.width, Screen.height, 0 );

        Client.Game.Batcher.UseDynamicAtlas = useDynamicAtlas;
        Client.Game.Batcher.UseGraphicsDrawTexture = useGraphicsDrawTexture;
        Client.Game.DrawUnity(UnityEngine.Time.deltaTime);
        Client.Game.Batcher.Reset();

        forceEnterWorld = false;

        if (saveAllTextures)
        {
	        /*
	        var texturesPath = Path.Combine(Application.persistentDataPath, "Textures");
	        Directory.CreateDirectory(texturesPath);
	        Client.Game.Batcher.SeenTextures.ToList().ForEach(texture =>
		        {
			        if (texture.UnityTexture is Texture2D unityTexture)
			        {
				        File.WriteAllBytes(Path.Combine(texturesPath, texture.Hash + ".png"), unityTexture.EncodeToPNG());
			        }
		        });
	        Debug.Log($"Saved {Client.Game.Batcher.SeenTextures.Count} textures to {texturesPath}");

	        Client.Game.Batcher.smallTexturesAtlas.Save();
	        */
        }

        saveAllTextures = false;
    }

    public void StartGame(ServerConfiguration config)
    {
	    CUOEnviroment.ExecutablePath = config.GetPathToSaveFiles();

	    //Load and adjust settings
	    Settings.GlobalSettings = JsonConvert.DeserializeObject<Settings>(Resources.Load<TextAsset>("settings").text);
	    Settings.GlobalSettings.IP = config.UoServerUrl;
	    //If connecting to UO Outlands, set shard type to 2 for outlands
	    Settings.GlobalSettings.ShardType = config.UoServerUrl.ToLower().Contains("ououtlands") ? 2 : 0;

	    if (Application.isMobilePlatform)
	    {
		    Settings.GlobalSettings.UltimaOnlineDirectory = CUOEnviroment.ExecutablePath;
		    Settings.GlobalSettings.ClientVersion = config.ClientVersion;
	    }
	    else
	    {
		    Settings.GlobalSettings.UltimaOnlineDirectory = config.ClientPathForUnityEditor;
		    //Empty client version loaded from settings.json so that CUO detects the actual version from client.exe
		    Settings.GlobalSettings.ClientVersion = "";
	    }

	    //This flag is tied to whether the GameCursor gets drawn, in a convoluted way
	    //On mobile platform, set this flag to true to prevent the GameCursor from being drawn
	    Settings.GlobalSettings.RunMouseInASeparateThread = Application.isMobilePlatform;

	    //Some mobile specific overrides need to be done on the Profile but they can only be done once the Profile has been loaded
	    ProfileManager.ProfileLoaded += OnProfileLoaded;

	    // Add an audio source and tell the media player to use it for playing sounds
	    MediaPlayer.AudioSourceOneShot = gameObject.AddComponent<AudioSource>();
	    Log.Start( LogTypes.All );

	    Client.Run();

	    generatedHueTexture1 = (Texture2D) Client.Game.GraphicsDevice.Textures[1].UnityTexture;
	    generatedHueTexture2 = (Texture2D) Client.Game.GraphicsDevice.Textures[2].UnityTexture;

	    Client.Game.sceneChanged += OnSceneChanged;
	    ApplyScalingFactor();
    }

    private void OnProfileLoaded()
    {
	    //Disable auto move on mobile platform
	    ProfileManager.Current.DisableAutoMove = Application.isMobilePlatform;
	    //Prevent stack split gump from appearing on mobile
	    ProfileManager.Current.HoldShiftToSplitStack = Application.isMobilePlatform;
	    //Scale items inside containers by default on mobile (won't have any effect if container scale isn't changed)
	    ProfileManager.Current.ScaleItemsInsideContainers = Application.isMobilePlatform;
    }

    private void OnSceneChanged()
    {
	    ApplyScalingFactor();
	    movementJoystick.gameObject.SetActive((Application.isMobilePlatform || showMovementJoystickOnNonMobilePlatforms) && Client.Game.Scene is GameScene);
    }

    private void ApplyScalingFactor()
    {
	    var scale = 1f;

	    if (Client.Game == null)
	    {
		    return;
	    }

	    var isGameScene = Client.Game.Scene is GameScene;

	    if (ScaleGameToFitScreen)
	    {
		    var loginScale = Mathf.Min(Screen.width / 640f, Screen.height / 480f);
		    var gameScale = Mathf.Max(1, loginScale * 0.75f);
		    scale = isGameScene ? gameScale : loginScale;
	    }

	    if (CustomScaleSize != ScaleSizes.Default && isGameScene)
	    {
		    scale *= (int)CustomScaleSize / 100f;
	    }

	    ((UnityGameWindow) Client.Game.Window).Scale = scale;
	    Client.Game.Batcher.scale = scale;
	    Client.Game.scale = scale;
    }

    private static readonly List<RaycastResult> tempRaycastResults = new List<RaycastResult>(10);
    private static PointerEventData tempPointerEventData;
    private static EventSystem tempEventSystem;

    private static bool PointOverGui(Vector2 screenPosition)
    {
	    tempRaycastResults.Clear();

	    var currentEventSystem = EventSystem.current;

	    // Create point event data for this event system?
	    if (currentEventSystem != tempEventSystem)
	    {
		    tempEventSystem = currentEventSystem;

		    if (tempPointerEventData == null)
		    {
			    tempPointerEventData = new PointerEventData(tempEventSystem);
		    }
		    else
		    {
			    tempPointerEventData.Reset();
		    }
	    }

	    // Raycast event system at the specified point
	    tempPointerEventData.position = screenPosition;

	    currentEventSystem.RaycastAll(tempPointerEventData, tempRaycastResults);

	    return tempRaycastResults.Count > 0;
    }
}
