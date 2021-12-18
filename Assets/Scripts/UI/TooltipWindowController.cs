using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct TooltipMessage
{
    public Color Color { get; set; }
    public string Message { get; set; }
    public int Font { get; set; }

    public TooltipMessage(Color color, string message, int font)
    {
        this.Color = color;
        this.Message = message;
        this.Font = font;
    }

    public TooltipMessage(Color color, string message)
    {
        this.Message = message;
        this.Color = color;
        this.Font = 14;
    }

    public TooltipMessage(string message, int font, int maxWordsInLine = 0)
    {
        this.Color = Color.white;
        this.Font = font;

        if (maxWordsInLine != 0)
        {
            string[] words = message.Split(' ');

            int wordCount = words.Length;

            int requiredLinesAmount = wordCount / maxWordsInLine;

            message = "";

            for (int i = 0; i < wordCount; i++)
            {
                message += words[i] + " ";
                if (i % 4 == 0 && i != 0 && i != wordCount - 1)
                {
                    message += System.Environment.NewLine;
                }
            }
        }

        this.Message = message;
    }

    public TooltipMessage(string message)
    {
        this.Color = Color.white;
        this.Font = 14;
        this.Message = message;
    }
}

public class TooltipWindowController : MonoBehaviour
{
    private static TooltipWindowController i;

    [SerializeField]
    private RectTransform customTextWindow;
    [SerializeField]
    private RectTransform spawningTextWindow;

    [SerializeField]
    private GameObject textPrefab;

    [SerializeField]
    private float offset;

    private bool isCursorFollowing = false;

    private void Awake()
    {
        if (i != null && i != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            i = this;
        }
    }

    private void Update()
    {
        if (this.isCursorFollowing)
        {
            i.customTextWindow.position = Input.mousePosition;
        }
    }

    public static void HideTooltip()
    {
        if (i == null)
            return;
        i.isCursorFollowing = false;
        i.customTextWindow.gameObject.SetActive(false);
    }

    public static void ShowTooltip(List<TooltipMessage> messages)
    {
        foreach (RectTransform previousText in i.spawningTextWindow)
        {
            Destroy(previousText.gameObject);
        }

        foreach (TooltipMessage message in messages)
        {
            GameObject currentText = Instantiate(i.textPrefab, i.spawningTextWindow);
            TMPro.TMP_Text textComponent = currentText.GetComponent<TMPro.TMP_Text>();
            textComponent.text = message.Message;
            textComponent.color = message.Color;
            textComponent.fontSize = message.Font;
            Canvas.ForceUpdateCanvases();
        }
        i.spawningTextWindow.gameObject.SetActive(true);
        i.customTextWindow.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)i.spawningTextWindow.transform);
        i.isCursorFollowing = true;

    }
}
