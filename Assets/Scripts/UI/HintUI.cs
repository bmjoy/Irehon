using UnityEngine;
using UnityEngine.UI;

public class HintUI : MonoBehaviour
{
    [SerializeField]
    private Text header;
    [SerializeField]
    private Text content;

    public void UpdateHint(string title, string hint)
    {
        this.header.text = title;
        this.content.text = hint;
    }
}
