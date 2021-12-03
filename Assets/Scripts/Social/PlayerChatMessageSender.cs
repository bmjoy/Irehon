using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerChatMessageSender : MonoBehaviour
{
    [SerializeField]
    private InputField chatInputField;


    private bool isBeenFocused;

    private void Update()
    {
        if (GameSession.IsListeningGameKeys && Input.GetKeyDown(KeyCode.Return))
            chatInputField.ActivateInputField();

        if (chatInputField.isFocused)
        {
            isBeenFocused = true;
            GameSession.IsListeningGameKeys = false;
        }
        if (isBeenFocused && !chatInputField.isFocused)
        {
            isBeenFocused = false;
            GameSession.IsListeningGameKeys = true;
        }
    }

    public void ChatMessage()
    {
        chatInputField.DeactivateInputField();
        if (chatInputField.text == "")
            return;

        PlayerChatHolder.instance.PlayerChatInputEvent.Invoke(chatInputField.text);
        chatInputField.text = "";
    }
}
