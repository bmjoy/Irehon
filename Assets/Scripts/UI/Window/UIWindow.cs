using Irehon;
using Irehon.UI;
using Sirenix.OdinInspector;
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
        this.windowObject = this.transform.GetChild(0).gameObject;
    }

    public void Open()
    {
        if (!this.isEnabled)
        {
            openedWindowsCount++;
        }

        this.isEnabled = true;
        this.windowObject.SetActive(true);
        this.OnOpenWindow?.Invoke();
        Mouse.EnableCursor();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.windowObject.transform);
    }

    public static void ResetWindowsCount() => openedWindowsCount = 0;

    public void Close()
    {
        if (this.isEnabled)
        {
            openedWindowsCount--;
        }

        if (openedWindowsCount == 0)
        {
            Mouse.DisableCursor();
        }

        this.isEnabled = false;
        this.windowObject.SetActive(false);
        this.OnCloseWindow?.Invoke();
        if (this.isClosingStoppingInterract)
        {
            Player.LocalPlayer?.GetComponent<PlayerInteracter>().StopInterractCommand();
        }
    }

    private void Update()
    {
        if (!GameSession.IsListeningGameKeys)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && updateWindowsCount == 0)
        {
            if (this.TriggerKey != KeyCode.Escape)
            {
                this.Close();
            }
            else
            {
                this.Open();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && updateWindowsCount != 0)
        {
            this.Close();
        }
        else if (Input.GetKeyDown(this.TriggerKey))
        {
            this.SwitchWindowState();
        }
    }

    private void LateUpdate()
    {
        updateWindowsCount = openedWindowsCount;
    }

    public void SwitchWindowState()
    {
        if (this.isEnabled)
        {
            this.Close();
        }
        else
        {
            this.Open();
        }
    }
}
