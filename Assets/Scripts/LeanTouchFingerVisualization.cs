using Lean.Touch;
using UnityEngine;

public class LeanTouchFingerVisualization : MonoBehaviour
{
    [SerializeField]
    private LeanTouch leanTouch;

    [SerializeField]
    private Texture2D fingerTexture;
    
    private void Awake()
    {
        UserPreferences.VisualizeFingerInput.ValueChanged += OnVisualizeFingerInputChanged;
        OnVisualizeFingerInputChanged(UserPreferences.VisualizeFingerInput.CurrentValue);
    }

    private void OnVisualizeFingerInputChanged(int currentValue)
    {
        var show = currentValue == (int) PreferenceEnums.VisualizeFingerInput.On;
        leanTouch.FingerTexture = show ? fingerTexture : null;
    }
}
