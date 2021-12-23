using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        Canvas[] canvas = this.GetComponentsInParent<Canvas>();
        this.canvas = canvas[canvas.Length - 1];

        this.rectTransform = this.GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.rectTransform.anchoredPosition += eventData.delta / this.canvas.scaleFactor;
    }
}
