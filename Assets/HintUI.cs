using System.Collections;
using System.Collections.Generic;
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
        header.text = title;
        content.text = hint;
    }
}
