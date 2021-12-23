using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonKeyListener : MonoBehaviour
{
    [SerializeField]
    private KeyCode listeningKey;
    private EventSystem system;
    private void Start()
    {
        this.system = EventSystem.current;
    }
    private void Update()
    {
        if (Input.GetKeyDown(this.listeningKey))
        {
            this.system.SetSelectedGameObject(this.gameObject, new BaseEventData(this.system));
            this.GetComponent<IPointerClickHandler>()?.OnPointerClick(new PointerEventData(this.system));
        }
    }
}
