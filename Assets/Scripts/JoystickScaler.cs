using UnityEngine;

public class JoystickScaler : MonoBehaviour
{
    [SerializeField]
    private MobileJoystick mobileJoystick;

    [SerializeField]
    private RectTransform joystickRectTransform;

    [SerializeField]
    private RectTransform canvasRectTransform;

    [SerializeField]
    private float minSize;

    [SerializeField]
    private float maxSize;
    
    [SerializeField]
    private float smallSize;
    
    [SerializeField]
    private float normalSize;
    
    [SerializeField]
    private float largeSize;

    private void Awake()
    {
        UserPreferences.JoystickSize.ValueChanged += OnJoystickSizeChanged;
        OnJoystickSizeChanged(UserPreferences.JoystickSize.CurrentValue);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        mobileJoystick.enabled = false;
    }

    private void OnDisable()
    {
        mobileJoystick.enabled = true;
    }

    public void UpdateSizeAndPositionFromCorners(JoystickScalerCorner corner, Vector2 diff)
    {
        var cornerDirection = corner.transform.localPosition;
        cornerDirection.x = Mathf.Sign(cornerDirection.x);
        cornerDirection.y = Mathf.Sign(cornerDirection.y);
        var originalSizeDelta = joystickRectTransform.sizeDelta;
        var newSizeDelta = originalSizeDelta + new Vector2(diff.x * cornerDirection.x, diff.y * cornerDirection.y);
        var minDimension = Mathf.Min(newSizeDelta.x, newSizeDelta.y);
        minDimension = Mathf.Clamp(minDimension, minSize, maxSize);
        mobileJoystick.SetSize(minDimension);
        var sizeDeltaDiff = joystickRectTransform.sizeDelta - originalSizeDelta;
        joystickRectTransform.localPosition += new Vector3(sizeDeltaDiff.x * 0.5f * cornerDirection.x, sizeDeltaDiff.y * 0.5f * cornerDirection.y, 0f);
    }

    public void Move(Vector2 diff)
    {
        var anchoredPosition = joystickRectTransform.anchoredPosition;
        anchoredPosition += diff;
        
        var extent = joystickRectTransform.sizeDelta * 0.5f;
        if (anchoredPosition.x - extent.x < 0)
        {
            anchoredPosition.x -= anchoredPosition.x - extent.x;
        }
        else if (anchoredPosition.x + extent.x > canvasRectTransform.sizeDelta.x)
        {
            anchoredPosition.x -= (anchoredPosition.x + extent.x) - canvasRectTransform.sizeDelta.x;
        }
        
        if (anchoredPosition.y - extent.y < 0)
        {
            anchoredPosition.y -= anchoredPosition.y - extent.y;
        }
        else if (anchoredPosition.y + extent.y > canvasRectTransform.sizeDelta.y)
        {
            anchoredPosition.y -= (anchoredPosition.y + extent.y) - canvasRectTransform.sizeDelta.y;
        }

        joystickRectTransform.anchoredPosition = anchoredPosition;
    }

    public void CornerDragEnded()
    {
        SaveSizeAndPosition();
    }

    public void MoveEnded()
    {
        SaveSizeAndPosition();
    }
    
    private void SaveSizeAndPosition()
    {
        UserPreferences.CustomJoystickPositionAndSize.CurrentValue = new Vector3(joystickRectTransform.anchoredPosition.x, joystickRectTransform.anchoredPosition.y, joystickRectTransform.sizeDelta.x);
        UserPreferences.JoystickSize.CurrentValue = (int) PreferenceEnums.JoystickSizes.Custom;
    }
    
    private void OnJoystickSizeChanged(int joystickSize)
    {
        var rectT = (RectTransform) mobileJoystick.transform;
        var sizeEnum = (PreferenceEnums.JoystickSizes) UserPreferences.JoystickSize.CurrentValue;
        var customJoystickPositionAndSize = UserPreferences.CustomJoystickPositionAndSize.CurrentValue;
        var size = rectT.sizeDelta.x;
        if (sizeEnum == PreferenceEnums.JoystickSizes.Small)
        {
            size = smallSize;
        }
        else if (sizeEnum == PreferenceEnums.JoystickSizes.Normal)
        {
            size = normalSize;
        }
        else if (sizeEnum == PreferenceEnums.JoystickSizes.Large)
        {
            size = largeSize;
        }
        else if (sizeEnum == PreferenceEnums.JoystickSizes.Custom)
        {
            if (customJoystickPositionAndSize.z != -1)
            {
                size = customJoystickPositionAndSize.z;
            }
        }
        mobileJoystick.SetSize(size);
        
        //Also update anchoredPosition if there's a custom position defined
        if (customJoystickPositionAndSize.x != -1)
        {
            ((RectTransform)mobileJoystick.transform).anchoredPosition = new Vector2(customJoystickPositionAndSize.x, customJoystickPositionAndSize.y);
        }
    }
}
