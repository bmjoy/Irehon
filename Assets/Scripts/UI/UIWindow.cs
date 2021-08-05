using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIWindow : MonoBehaviour
{
    [SerializeField]
    private KeyCode TriggerKey;
    private bool isEnabled;

    public UnityEvent OnCloseWindow;

    public UnityEvent OnOpenWindow;

    private GameObject windowObject;
    private LayoutGroup layoutGroup;

    private void Awake()
    {
        windowObject = transform.GetChild(0).gameObject;
        layoutGroup = windowObject.GetComponent<HorizontalLayoutGroup>();
    }

    public void Open()
    {
        isEnabled = true;
        Canvas.ForceUpdateCanvases();
        layoutGroup.enabled = false;
        layoutGroup.enabled = true;
        windowObject.SetActive(true);
        OnOpenWindow?.Invoke();
    }

    public void Close()
    {
        isEnabled = false;
        windowObject.SetActive(false);
        OnCloseWindow?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(TriggerKey))
            SwitchWindowState();
    }

    public void SwitchWindowState()
    {
        isEnabled = !isEnabled;
        if (isEnabled)
            Open();
        else
            Close();
    }
}
