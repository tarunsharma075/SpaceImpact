using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Reads the existing JoyStick image and moves its child thumb in four directions.</summary>
public sealed class GameplayControlsUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform thumb;
    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.12f;

    public static Vector2 Movement { get; private set; }

    private RectTransform baseRect;
    private float travelRadius;
    private int activePointerId = int.MinValue;

    private void Awake()
    {
        baseRect = transform as RectTransform;
        if (thumb == null)
        {
            var child = transform.Find("Button");
            if (child != null) thumb = child as RectTransform;
        }

        if (baseRect == null || thumb == null)
        {
            Debug.LogError("JoyStick needs a RectTransform and a child thumb Button.", this);
            enabled = false;
            return;
        }

        float diameter = Mathf.Min(baseRect.rect.width, baseRect.rect.height);
        // Keep the thumb's size and anchors exactly as authored in the Game scene.
        // The sprite has transparent margins, so its visible circle is smaller than its RectTransform.
        travelRadius = diameter * 0.26f;

        // Let the outer joystick receive pointer events even when the thumb is beneath the finger.
        var thumbImage = thumb.GetComponent<Image>();
        if (thumbImage != null) thumbImage.raycastTarget = false;
        var baseImage = GetComponent<Image>();
        if (baseImage != null) baseImage.raycastTarget = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue && activePointerId != eventData.pointerId) return;
        activePointerId = eventData.pointerId;
        UpdateThumb(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId) UpdateThumb(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        activePointerId = int.MinValue;
        Movement = Vector2.zero;
        if (thumb != null) thumb.anchoredPosition = Vector2.zero;
    }

    private void UpdateThumb(PointerEventData eventData)
    {
        if (baseRect == null || thumb == null || travelRadius <= 0f) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                baseRect, eventData.position, eventData.pressEventCamera, out var localPoint)) return;

        Vector2 offset = localPoint - baseRect.rect.center;
        float distance = Mathf.Min(offset.magnitude, travelRadius);
        if (distance <= travelRadius * deadZone)
        {
            Movement = Vector2.zero;
            thumb.anchoredPosition = Vector2.zero;
            return;
        }

        // Snap to the nearest cardinal axis; the thumb and ship never move diagonally.
        Vector2 direction = Mathf.Abs(offset.x) > Mathf.Abs(offset.y)
            ? new Vector2(Mathf.Sign(offset.x), 0f)
            : new Vector2(0f, Mathf.Sign(offset.y));

        float strength = distance / travelRadius;
        thumb.anchoredPosition = direction * distance;
        Movement = direction * strength;
    }

    private void OnDisable()
    {
        activePointerId = int.MinValue;
        Movement = Vector2.zero;
        if (thumb != null) thumb.anchoredPosition = Vector2.zero;
    }
}
