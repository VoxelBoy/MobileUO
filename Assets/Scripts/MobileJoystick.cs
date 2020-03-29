using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private RectTransform Background;
    [SerializeField]
    private RectTransform Knob;
    [SerializeField]
    private float offset;
    public Vector2 Position { get; private set; }

    public void OnDrag(PointerEventData eventData)
    {
        var backgroundRect = Background.rect;
        var knobRect = Knob.rect;
        var backgroundPosition = (Vector2) Background.position;

        var eventToBackgroundPositionDelta = eventData.position - backgroundPosition;
        var backgroundToKnobRectSizeDelta = backgroundRect.size * 0.5f;

        Position = new Vector2(eventToBackgroundPositionDelta.x / (backgroundToKnobRectSizeDelta.x), eventToBackgroundPositionDelta.y / (backgroundToKnobRectSizeDelta.y));
        Position = Position.sqrMagnitude > 1.0f ? Position.normalized : Position;
        Knob.position = new Vector2(Position.x * (backgroundToKnobRectSizeDelta.x) * offset + backgroundPosition.x, Position.y * (backgroundToKnobRectSizeDelta.y) * offset + backgroundPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Position = Vector2.zero;
        Knob.position = Background.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnEndDrag(eventData);
    }
}