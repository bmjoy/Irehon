using UnityEngine;
using UnityEngine.EventSystems;

public class Focussable : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Transform target;
    public void OnPointerClick(PointerEventData eventData)
    {
        this.target.SetAsLastSibling();
    }
}
