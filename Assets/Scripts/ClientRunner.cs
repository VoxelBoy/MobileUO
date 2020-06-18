using System;
using System.IO;
using System.Linq;
using UnityEngine;
using ClassicUO;
using ClassicUO.Utility.Logging;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Game.UI.Gumps.Login;
using Newtonsoft.Json;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

public class ClientRunner : MonoBehaviour
{
	[SerializeField]
	private bool useDynamicAtlas;
	[SerializeField]
	public bool useGraphicsDrawTexture;
	[SerializeField]
	private bool forceEnterWorld;
	[SerializeField]
	private bool saveAllTextures;
	[SerializeField]
	private bool scaleGameToFitScreen;
	
	[SerializeField]
	private MobileJoystick movementJoystick;
	[SerializeField]
	private bool showMovementJoystickOnNonMobilePlatforms;

	private int lastScreenWidth;
	private int lastScreenHeight;
	
	public Action<string> OnError;
	public Action OnExiting;
	public Action<bool> SceneChanged;

	private void Awake()
	{
		UserPreferences.ScaleSize.ValueChanged += OnCustomScaleSizeChanged;
		UserPreferences.ShowCloseButtons.ValueChanged += OnShowCloseButtonsChanged;
		UserPreferences.UseMouseOnMobile.ValueChanged += OnUseMouseOnMobileChanged;
		UserPreferences.TargetFrameRate.ValueChanged += OnTargetFrameRateChanged;
		UserPreferences.TextureFiltering.ValueChanged += UpdateTextureFiltering;
		OnCustomScaleSizeChanged(UserPreferences.ScaleSize.CurrentValue);
		OnShowCloseButtonsChanged(UserPreferences.ShowCloseButtons.CurrentValue);
		OnUseMouseOnMobileChanged(UserPreferences.UseMouseOnMobile.CurrentValue);
		OnTargetFrameRateChanged(UserPreferences.TargetFrameRate.CurrentValue);
		UpdateTextureFiltering(UserPreferences.TextureFiltering.CurrentValue);
	}
	
	private static void OnTargetFrameRateChanged(int frameRate)
	{
		Application.targetFrameRate = frameRate;
	}
    
	private void UpdateTextureFiltering(int textureFiltering)
	{
		var filterMode = (FilterMode) textureFiltering;
		Texture2D.defaultFilterMode = filterMode;
		if (Client.Game != null)
		{
			var textures = FindObjectsOfType<Texture>();
			foreach (var t in textures)
			{
				t.filterMode = filterMode;
			}
			Client.Game.GraphicsDevice.Textures[1].UnityTexture.filterMode = FilterMode.Point;
			Client.Game.GraphicsDevice.Textures[2].UnityTexture.filterMode = FilterMode.Point;
		}
	}
	
	private void OnUseMouseOnMobileChanged(int useMouse)
	{
		UpdateMovementJoystick();
	}

	private void OnCustomScaleSizeChanged(int customScaleSize)
	{
		ApplyScalingFactor();
	}
	
	private void OnShowCloseButtonsChanged(int showCloseButtons)
	{
		Gump.CloseButtonsEnabled = showCloseButtons != 0;
        
		foreach (var control in UIManager.Gumps)
		{
			if (control is Gump gump)
			{
				gump.UpdateCloseButton();
			}
		}
	}

	private void Update ()
	{
		if (Client.Game == null)
			return;

		if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
		{
			lastScreenWidth = Screen.width;
			lastScreenHeight = Screen.height;
			ApplyScalingFactor();
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
			World.Player.AddToTile();
			Client.Game.SetScene(new GameScene());
		}

		float deltaTime = UnityEngine.Time.deltaTime;
		//Is this necessary? Wouldn't it slow down the game even further when it dips below 20 FPS?
        if(deltaTime > 0.050f)
        {
            deltaTime = 0.050f;
        }

        if (movementJoystick.isActiveAndEnabled && Client.Game.Scene is GameScene gameScene)
        {
	        gameScene.JoystickInput = new Microsoft.Xna.Framework.Vector2(movementJoystick.Position.x, -1 * movementJoystick.Position.y);
        }
        
        Client.Game.Tick(deltaTime);
	}

	private void OnPostRender()
    {
	    if (Client.Game == null)
		    return;

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
	    var settingsFilePath = Settings.GetSettingsFilepath();
	    if (File.Exists(settingsFilePath))
	    {
		    Settings.GlobalSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath));
	    }
	    else
	    {
		    Settings.GlobalSettings = JsonConvert.DeserializeObject<Settings>(Resources.Load<TextAsset>("settings").text);
	    }

	    Settings.GlobalSettings.IP = config.UoServerUrl;
	    Settings.GlobalSettings.Port = ushort.Parse(config.UoServerPort);
	    
	    //Reset static encryption type variable
	    EncryptionHelper.Type = ENCRYPTION_TYPE.NONE;
	    Settings.GlobalSettings.Encryption = (byte) (config.UseEncryption ? 1 : 0);

	    //Empty the plugins array because no plugins are working at the moment
	    Settings.GlobalSettings.Plugins = new string[0];
	    
	    //If connecting to UO Outlands, set shard type to 2 for outlands
	    Settings.GlobalSettings.ShardType = config.UoServerUrl.ToLower().Contains("ououtlands") ? 2 : 0;

	    //Try to detect old client version to set ShardType to 1, for using StatusGumpOld. Otherwise, it's possible
	    //to get null-refs in StatusGumpModern.
	    if (ClientVersionHelper.IsClientVersionValid(config.ClientVersion, out var clientVersion))
	    {
		    if (clientVersion < ClientVersion.CV_308Z)
		    {
			    Settings.GlobalSettings.ShardType = 1;
		    }
	    }
	    
	    CUOEnviroment.IsOutlands = Settings.GlobalSettings.ShardType == 2;

	    Settings.GlobalSettings.ClientVersion = config.ClientVersion;
	    
	    if (Application.isMobilePlatform == false && string.IsNullOrEmpty(config.ClientPathForUnityEditor) == false)
	    {
		    Settings.GlobalSettings.UltimaOnlineDirectory = config.ClientPathForUnityEditor;
	    }
	    else
	    {
		    Settings.GlobalSettings.UltimaOnlineDirectory = config.GetPathToSaveFiles();
	    }

	    //This flag is tied to whether the GameCursor gets drawn, in a convoluted way
	    //On mobile platform, set this flag to true to prevent the GameCursor from being drawn
	    Settings.GlobalSettings.RunMouseInASeparateThread = Application.isMobilePlatform;

	    //Some mobile specific overrides need to be done on the Profile but they can only be done once the Profile has been loaded
	    ProfileManager.ProfileLoaded += OnProfileLoaded;

	    // Add an audio source and tell the media player to use it for playing sounds
	    MediaPlayer.AudioSourceOneShot = gameObject.AddComponent<AudioSource>();
	    Log.Start( LogTypes.All );

	    try
	    {
		    Client.SceneChanged += OnSceneChanged;
		    Client.Run();
		    Client.Game.Exiting += OnGameExiting;
		    ApplyScalingFactor();
	    }
	    catch (Exception e)
	    {
		    Console.WriteLine(e);
		    OnError?.Invoke(e.ToString());
	    }
    }

    public static void Login()
    {
	    if (Client.Game == null || !(Client.Game.Scene is LoginScene loginScene) || loginScene.CurrentLoginStep != LoginSteps.Main)
	    {
		    return;
	    }
	    var loginGump = UIManager.Gumps.FirstOrDefault(g => g is LoginGump) as LoginGump;
	    loginGump?.OnButtonClick((int) LoginGump.Buttons.NextArrow);
    }

    private void OnProfileLoaded()
    {
	    //Disable auto move on mobile platform
	    ProfileManager.Current.DisableAutoMove = Application.isMobilePlatform;
	    //Prevent stack split gump from appearing on mobile
	    //ProfileManager.Current.HoldShiftToSplitStack = Application.isMobilePlatform;
	    //Scale items inside containers by default on mobile (won't have any effect if container scale isn't changed)
	    ProfileManager.Current.ScaleItemsInsideContainers = Application.isMobilePlatform;
    }

    private void OnSceneChanged()
    {
	    ApplyScalingFactor();
	    UpdateMovementJoystick();
	    SceneChanged?.Invoke(Client.Game.Scene is GameScene);
    }

    private void UpdateMovementJoystick()
    {
	    movementJoystick.gameObject.SetActive((Application.isMobilePlatform || showMovementJoystickOnNonMobilePlatforms)
	                                          && Client.Game != null && Client.Game.Scene is GameScene
	                                          && UserPreferences.UseMouseOnMobile.CurrentValue == 0);
    }

    private void ApplyScalingFactor()
    {
	    var scale = 1f;

	    if (Client.Game == null)
	    {
		    return;
	    }

	    var gameScene = Client.Game.Scene as GameScene;
	    var isGameScene = gameScene != null;

	    if (scaleGameToFitScreen)
	    {
		    var loginScale = Mathf.Min(Screen.width / 640f, Screen.height / 480f);
		    var gameScale = Mathf.Max(1, loginScale * 0.75f);
		    scale = isGameScene ? gameScale : loginScale;
	    }

	    if (UserPreferences.ScaleSize.CurrentValue != (int) PreferenceEnums.ScaleSizes.Default && isGameScene)
	    {
		    scale *= UserPreferences.ScaleSize.CurrentValue / 100f;
	    }

	    ((UnityGameWindow) Client.Game.Window).Scale = scale;
	    Client.Game.Batcher.scale = scale;
	    Client.Game.scale = scale;
	    
	    //Force update game viewport render texture
	    if (isGameScene)
	    {
		    gameScene.UpdateDrawPosition = true;
		    gameScene.GetViewPort();
	    }
    }

    private void OnGameExiting(object sender, EventArgs e)
    {
	    Client.Game.UnloadContent();
	    Client.Game.Dispose();
	    OnExiting?.Invoke();
    }
}
