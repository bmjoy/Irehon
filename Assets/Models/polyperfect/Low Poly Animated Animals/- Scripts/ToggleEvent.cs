using UnityEngine;

public class ToggleEvent : MonoBehaviour
{

    public UnityEngine.Events.UnityEvent toggleOn, toggleOff;
    private UnityEngine.UI.Toggle toggle;

    private void Awake()
    {
        this.toggle = this.GetComponent<UnityEngine.UI.Toggle>();

        this.toggle.onValueChanged.AddListener((value) => this.SwapToggle(this.toggle.isOn));
    }

    public void SwapToggle(bool value)
    {
        switch (value)
        {
            case true:
                this.toggleOn.Invoke();
                break;
            case false:
                this.toggleOff.Invoke();
                break;
        }
    }


}
