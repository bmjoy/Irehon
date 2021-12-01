using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerChatMessageSender : MonoBehaviour
{
    [SerializeField]
    private InputField chatInputField;

    private void Update()
    {
        PlayerInput.IsPlayerListeningInput = !chatInputField.isFocused;
    }

    public void ChatMessage()
    {
        if (chatInputField.text == "")
            return;

        PlayerChatHolder.instance.PlayerChatInputEvent.Invoke(chatInputField.text);
        chatInputField.text = "";
    }
}
