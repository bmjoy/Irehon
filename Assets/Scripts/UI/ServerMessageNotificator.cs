using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Server;

public class ServerMessageNotificator : MonoBehaviour
{
    [SerializeField]
    private Text textComponent;

    private static ServerMessageNotificator i;
    private Image parentBG;
    private Coroutine opacity;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(gameObject);
        else
            i = this;
        DontDestroyOnLoad(gameObject);

        parentBG = textComponent.transform.parent.GetComponent<Image>();
    }

    private IEnumerator DisappearHitMarker()
    {
        yield return new WaitForSeconds(1.5f);
        while (textComponent.color.a > 0)
        {
            textComponent.color = new Color(1, 1, 1, textComponent.color.a - 0.25f);
            parentBG.color = new Color(1, 1, 1, textComponent.color.a - 0.25f);
            yield return new WaitForSeconds(.1f);
        }
        textComponent.text = "";
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)textComponent.transform);
    }

    public static void ShowMessage(ServerMessage msg)
    {
        i.MessageShowAndHide(msg);
    }

    private void MessageShowAndHide(ServerMessage msg)
    {
        if (opacity != null)
            StopCoroutine(i.opacity);
        textComponent.text += msg.message + System.Environment.NewLine;
        textComponent.color = Color.white;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)textComponent.transform);
        opacity = StartCoroutine(DisappearHitMarker());
    }
}
