using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Focussable : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Transform target;
    public void OnPointerClick(PointerEventData eventData)
    {
        target.SetAsLastSibling();
    }
}
