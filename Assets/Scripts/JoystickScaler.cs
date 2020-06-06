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
        UserPreferences.CustomJoystickPositionAndSize = new Vector3(joystickRectTransform.anchoredPosition.x, joystickRectTransform.anchoredPosition.y, joystickRectTransform.sizeDelta.x);
        UserPreferences.JoystickSize = UserPreferences.JoystickSizes.Custom;
    }
}
