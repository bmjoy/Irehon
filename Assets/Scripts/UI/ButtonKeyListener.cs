using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonKeyListener : MonoBehaviour
{
    [SerializeField]
    private KeyCode listeningKey;
    private EventSystem system;
    private void Start()
    {
        system = EventSystem.current;
    }
    private void Update()
    {
        if (Input.GetKeyDown(listeningKey))
        {
            system.SetSelectedGameObject(gameObject, new BaseEventData(system));
            GetComponent<IPointerClickHandler>()?.OnPointerClick(new PointerEventData(system));
        }
    }
}
