using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class OnScreenStickExtension : OnScreenStick, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] RectTransform _outerRing;
    private Image _outerRingGraphics;
    private Image _innerCircleGraphics;

    private void Awake()
    {
        _innerCircleGraphics = GetComponent<Image>();
        _outerRingGraphics = _outerRing.GetComponent<Image>();
    }

    public new void OnPointerDown(PointerEventData pointerEventData)
    {
        _outerRing.position = pointerEventData.position;

        _innerCircleGraphics.enabled = true;
        _outerRingGraphics.enabled = true;
        base.OnPointerDown(pointerEventData);
    }

    public new void OnPointerUp(PointerEventData pointerEventData)
    {
        _innerCircleGraphics.enabled = false;
        _outerRingGraphics.enabled = false;
        base.OnPointerUp(pointerEventData);
    }
}
