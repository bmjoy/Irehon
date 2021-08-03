using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindow : MonoBehaviour
{
    [SerializeField]
    private KeyCode TriggerKey;
    private bool isEnabled;

    private GameObject windowObject;

    private void Awake()
    {
        windowObject = transform.GetChild(0).gameObject;
    }

    public void Open()
    {
        isEnabled = true;
        windowObject.SetActive(true);
    }

    public void Close()
    {
        isEnabled = false;
        windowObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(TriggerKey))
            SwitchWindowState();
    }

    public void SwitchWindowState()
    {
        isEnabled = !isEnabled;
        windowObject.SetActive(isEnabled);
    }
}
