using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TooltipMessage
{
    public Color Color { get; set; }
    public string Message { get; set; }
    public int Font { get; set; }

    public TooltipMessage(Color color, string message, int font)
    {
        Color = color;
        Message = message;
        Font = font;
    }

    public TooltipMessage(Color color, string message)
    {
        Message = message;
        Color = color;
        Font = 14;
    }

    public TooltipMessage(string message, int font)
    {
        Color = Color.white;
        Font = font;
        Message = message;
    }

    public TooltipMessage(string message)
    {
        Color = Color.white;
        Font = 14;
        Message = message;
    }
}

public class TooltipWindowController : MonoBehaviour
{
    private static TooltipWindowController i;

    [SerializeField]
    private RectTransform customTextWindow;

    [SerializeField]
    private GameObject textPrefab;

    private bool isCursorFollowing = false;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(gameObject);
        else
            i = this;
    }

    public static void HideTooltip()
    {
        i.customTextWindow.gameObject.SetActive(false);
    }

    public static void ShowTooltip(List<TooltipMessage> messages, RectTransform originalWindow)
    {
        foreach (RectTransform previousText in i.customTextWindow)
            Destroy(previousText.gameObject);

        foreach (TooltipMessage message in messages)
        {
            GameObject currentText = Instantiate(i.textPrefab, i.customTextWindow);
            var textComponent = currentText.GetComponent<TMPro.TMP_Text>();
            textComponent.text = message.Message;
            textComponent.color = message.Color;
            textComponent.fontSize = message.Font;
            i.customTextWindow.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }

        Vector2 newAnchorPos = originalWindow.position;

        newAnchorPos.x -= i.customTextWindow.rect.width / 2;
        newAnchorPos.x -= originalWindow.rect.width / 2.5f;

        newAnchorPos.y += i.customTextWindow.rect.height / 2;
        newAnchorPos.y += originalWindow.rect.height / 2.5f;

        i.customTextWindow.position = new Vector3(newAnchorPos.x, newAnchorPos.y);
    }
}
