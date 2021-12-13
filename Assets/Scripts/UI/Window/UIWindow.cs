using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum WindowAction { Open, Close }

public class UIWindow : SerializedMonoBehaviour
{
    [SerializeField]
    private KeyCode TriggerKey;
    [SerializeField]
    private bool isClosingStoppingInterract;
    private bool isEnabled;

    public UnityEvent OnCloseWindow;

    public UnityEvent OnOpenWindow;

    private GameObject windowObject;

    private static int openedWindowsCount = 0;
    private static int updateWindowsCount = 0;

    private void Awake()
    {
        windowObject = transform.GetChild(0).gameObject;
    }

    public void Open()
    {
        if (!isEnabled)
            openedWindowsCount++;

        isEnabled = true;
        windowObject.SetActive(true);
        OnOpenWindow?.Invoke();
        CameraController.EnableCursor();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)windowObject.transform);
    }

    public void Close()
    {
        if (isEnabled)
            openedWindowsCount--;
        
        if (openedWindowsCount == 0)
            CameraController.DisableCursor();

        isEnabled = false;
        windowObject.SetActive(false);
        OnCloseWindow?.Invoke();
        if (isClosingStoppingInterract)
            ContainerWindowManager.i.StopInterractOnServer();
        
    }

    private void Update()
    {
        if (!GameSession.IsListeningGameKeys)
            return;
        if (Input.GetKeyDown(KeyCode.Escape) && updateWindowsCount == 0)
        {
            if (TriggerKey != KeyCode.Escape)
                Close();
            else
                Open();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && updateWindowsCount != 0)
        {
            Close();
        }
        else if (Input.GetKeyDown(TriggerKey))
        {
            SwitchWindowState();
        }
    }

    private void LateUpdate()
    {
        updateWindowsCount = openedWindowsCount;
    }

    public void SwitchWindowState()
    {
        if (isEnabled)
            Close();
        else
            Open();
    }
}
