using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private RectTransform background;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private RectTransform handle;
    [SerializeField]
    private float offset;
    public Vector2 Position { get; private set; }

    public void OnDrag(PointerEventData eventData)
    {
        var backgroundRect = background.rect;
        var backgroundPosition = (Vector2) background.position;

        var eventToBackgroundPositionDelta = eventData.position - backgroundPosition;
        var backgroundToKnobRectSizeDelta = backgroundRect.size * 0.5f;

        Position = new Vector2(eventToBackgroundPositionDelta.x / (backgroundToKnobRectSizeDelta.x), eventToBackgroundPositionDelta.y / (backgroundToKnobRectSizeDelta.y));
        Position = Position.sqrMagnitude > 1.0f ? Position.normalized : Position;
        handle.position = new Vector2(Position.x * (backgroundToKnobRectSizeDelta.x) * offset + backgroundPosition.x, Position.y * (backgroundToKnobRectSizeDelta.y) * offset + backgroundPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Position = Vector2.zero;
        handle.position = background.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnEndDrag(eventData);
    }

    public void SetSize(float size)
    {
        background.sizeDelta = Vector2.one * size;
        handle.sizeDelta = Vector2.one * size * 0.5f;
    }

    private void OnEnable()
    {
        backgroundImage.raycastTarget = true;
    }

    private void OnDisable()
    {
        backgroundImage.raycastTarget = false;
    }
}