using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Client;

public class KillLogger : MonoBehaviour
{
    private bool isIntialized;
    [SerializeField]
    private GameObject killLogTabPrefab;
    [SerializeField]
    private Transform logParent;
    private void Start()
    {
        if (!NetworkClient.isConnected)
            return;

        ClientManager.OnGetServerMessage.AddListener(RecieveServerMessage);
        isIntialized = true;
    }

    private void RecieveServerMessage(ServerMessage message)
    {
        if (message.messageType != MessageType.KillLog)
            return;

        ShowKill(ulong.Parse(message.message), ulong.Parse(message.subMessage));
    }

    public void ShowKill(ulong murder, ulong killed)
    {
        var log = Instantiate(killLogTabPrefab, logParent);
        log.GetComponent<KillLogTab>().Intialize(murder, killed);
    }

    private void OnDestroy()
    {
        if (isIntialized)
        {
            ClientManager.OnGetServerMessage.RemoveListener(RecieveServerMessage);
        }
    }
}
