using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        Canvas[] canvas = GetComponentsInParent<Canvas>();
        this.canvas = canvas[canvas.Length - 1];

        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
