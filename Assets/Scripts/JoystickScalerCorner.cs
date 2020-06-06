using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickScalerCorner : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private JoystickScaler joystickScaler;

    private RectTransform rectTransform;
    private int pointerId = -1;
    private Vector2 dragBeginPosition;

    private void OnEnable()
    {
        rectTransform = transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isActiveAndEnabled == false)
        {
            return;
        }
        pointerId = eventData.pointerId;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out dragBeginPosition);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (pointerId != eventData.pointerId || isActiveAndEnabled == false)
        {
            return;
        }
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out var newPosition);
        var diff = newPosition - dragBeginPosition;
        joystickScaler.UpdateSizeAndPositionFromCorners(this, diff);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isActiveAndEnabled == false)
        {
            return;
        }

        pointerId = -1;
        joystickScaler.CornerDragEnded();
    }
}
