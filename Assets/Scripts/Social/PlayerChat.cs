using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerChat : NetworkBehaviour
{
    private float lastMessageTimer;

    private void Start()
    {
        if (isLocalPlayer)
        {
            PlayerChatHolder.instance.PlayerChatInputEvent.AddListener(SendMessageToServer);
        }
    }

    private void FixedUpdate()
    {
        if (lastMessageTimer > 0)
            lastMessageTimer -= Time.fixedDeltaTime;
    }

    private void SendMessageToServer(string message)
    {
        SendChatMessage(message);
    }

    [Command]
    public void SendChatMessage(string message)
    {
        if (lastMessageTimer > 0)
        {
            Server.ServerManager.SendMessage(connectionToClient, "Don't spam messages", Client.MessageType.Notification);
            return;
        }
        lastMessageTimer = 0.75f;
        Server.ServerManager.Log(GetComponent<Player>().Id, $"Chat message: {message}");
        RecieveChatMessageRpc(message);
    }

    [ClientRpc]
    private void RecieveChatMessageRpc(string message)
    {
        PlayerChatHolder.instance.ShowMessage(GetComponent<Player>().Id, message);
    }
}
