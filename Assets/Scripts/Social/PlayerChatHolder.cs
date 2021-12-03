using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class PlayerChatInputEvent : UnityEvent<string> { }
public class PlayerChatHolder : MonoBehaviour
{
    public static PlayerChatHolder instance;

    public PlayerChatInputEvent PlayerChatInputEvent { get; private set; } = new PlayerChatInputEvent();

    [SerializeField]
    private GameObject messagesContentHolder;
    [SerializeField]
    private GameObject chatTabPrefab;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    public void ShowMessage(ulong steamId, string message)
    {
        var newMessage = Instantiate(chatTabPrefab, messagesContentHolder.transform);
        newMessage.GetComponent<PlayerChatMessageIntializer>().Intiallize(steamId, message);
    }
}
