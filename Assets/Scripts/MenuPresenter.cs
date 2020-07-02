using DG.Tweening;
using PreferenceEnums;
using UnityEngine;
using UnityEngine.UI;

public class MenuPresenter : MonoBehaviour
{
    [SerializeField]
    private Button menuButton;

    [SerializeField]
    private RectTransform listTransform;
    
    [SerializeField]
    private Vector3 listOpenPosition;
    
    [SerializeField]
    private Vector3 listClosedPosition;

    [SerializeField]
    private float listTweenDuration;

    private bool menuOpened;
    
    [SerializeField] private OptionEnumView optionEnumViewInstance;
    [SerializeField] private GameObject customizeJoystickButtonGameObject;
    [SerializeField] private GameObject loginButtonGameObject;
    [SerializeField] private ClientRunner clientRunner;

    void Awake()
    {
        menuButton.onClick.AddListener(OnMenuButtonClicked);
        
        GetOptionEnumViewInstance().Initialize(typeof(ShowCloseButtons), UserPreferences.ShowCloseButtons, "Close Buttons", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(ScaleSizes), UserPreferences.ScaleSize, "View Scale", true, true);
        GetOptionEnumViewInstance().Initialize(typeof(ForceUseXbr), UserPreferences.ForceUseXbr, "Force Use Xbr", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(TargetFrameRates), UserPreferences.TargetFrameRate, "Target Frame Rate", true, false);
        GetOptionEnumViewInstance().Initialize(typeof(TextureFilterMode), UserPreferences.TextureFiltering, "Texture Filtering", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(ContainerItemSelection), UserPreferences.ContainerItemSelection, "Container Item Selection", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(UseMouseOnMobile), UserPreferences.UseMouseOnMobile, "Use Mouse", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(JoystickSizes), UserPreferences.JoystickSize, "Joystick Size", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(JoystickOpacity), UserPreferences.JoystickOpacity, "Joystick Opacity", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(JoystickDeadZone), UserPreferences.JoystickDeadZone, "Joystick DeadZone", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(JoystickRunThreshold), UserPreferences.JoystickRunThreshold, "Joystick Run Threshold", false, false);
        GetOptionEnumViewInstance().Initialize(typeof(VisualizeFingerInput), UserPreferences.VisualizeFingerInput, "Visualize Finger Input", false, false);
        
        //Only show customize joystick button when UO client is running and we're in the game scene
        customizeJoystickButtonGameObject.transform.SetAsLastSibling();
        customizeJoystickButtonGameObject.SetActive(false);
        
        //Only show login button when UO client is running and we're in the login scene
        loginButtonGameObject.transform.SetAsFirstSibling();
        loginButtonGameObject.SetActive(false);
        
        clientRunner.SceneChanged += OnUoSceneChanged;
        
        optionEnumViewInstance.gameObject.SetActive(false);
    }

    private void OnUoSceneChanged(bool isGameScene)
    {
        customizeJoystickButtonGameObject.SetActive(isGameScene);
        loginButtonGameObject.SetActive(isGameScene == false);
    }

    private OptionEnumView GetOptionEnumViewInstance()
    {
        return Instantiate(optionEnumViewInstance.gameObject, optionEnumViewInstance.transform.parent).GetComponent<OptionEnumView>();
    }

    private void OnMenuButtonClicked()
    {
        menuOpened = !menuOpened;

        DOTween.Kill(listTransform);

        if (menuOpened)
        {
            listTransform.DOLocalMove(listOpenPosition, listTweenDuration);
        }
        else
        {
            listTransform.DOLocalMove(listClosedPosition, listTweenDuration);
        }
    }
}
