using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginSceneUI : MonoBehaviour
{
    public static InputField LoginField { get; private set; }
    public static InputField PasswordField { get; private set; }

    [SerializeField]
    private InputField loginField;
    [SerializeField]
    private InputField passwordField;

    public void Login() => Client.ClientManager.i.GetComponent<Client.ClientAuth>().LoginButton();
    public void Register() => Client.ClientManager.i.GetComponent<Client.ClientAuth>().RegisterButton();

    EventSystem system;

    void Start()
    {
        system = EventSystem.current;
        LoginField = loginField;
        PasswordField = passwordField;
    }

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {
                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null) 
                    inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
            else if ((next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp()) != null)
            {
                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null) 
                    inputfield.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
            else
                Debug.Log("next nagivation element not found");
        }
    }
}
