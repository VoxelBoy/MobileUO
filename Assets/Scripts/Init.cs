using ClassicUO;
using UnityEngine;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

public class Init : MonoBehaviour
{
    [SerializeField]
    private GameObject serverConfigurationUiParent;
    
    [SerializeField]
    private DownloadPresenter downloadPresenter;

    [SerializeField]
    private bool forceDownloadsInEditor;
    
    [SerializeField]
    private UnityMain unityMain;

    [SerializeField]
    private ErrorPresenter errorPresenter;

    [SerializeField]
    private Canvas inGameDebugConsoleCanvas;
    
    private void Awake()
    {
        ConsoleRedirect.Redirect();
        
        UserPreferences.TargetFrameRateChanged += OnTargetFrameRateChanged;
        UserPreferences.TextureFilteringChanged += UpdateTextureFiltering;
        UserPreferences.Initialize();

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter, forceDownloadsInEditor, inGameDebugConsoleCanvas));
        StateManager.AddState(new GameState(unityMain, errorPresenter, inGameDebugConsoleCanvas));

        inGameDebugConsoleCanvas.enabled = false;
        
        ServerConfigurationModel.Initialize();
        
        StateManager.GoToState<BootState>();
    }

    private static void OnTargetFrameRateChanged()
    {
        Application.targetFrameRate = (int) UserPreferences.TargetFrameRate;
    }
    
    private void UpdateTextureFiltering()
    {
        var filterMode = UserPreferences.TextureFiltering;
        Texture2D.defaultFilterMode = filterMode;
        if (Client.Game != null)
        {
            var textures = FindObjectsOfType<Texture>();
            for(int i=0; i<textures.Length; i++)
            {
                textures[i].filterMode = filterMode;
            }
            Client.Game.GraphicsDevice.Textures[1].UnityTexture.filterMode = FilterMode.Point;
            Client.Game.GraphicsDevice.Textures[2].UnityTexture.filterMode = FilterMode.Point;
        }
    }
}
