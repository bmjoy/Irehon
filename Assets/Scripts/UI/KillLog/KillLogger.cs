using Irehon.Client;
using Mirror;
using UnityEngine;

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
        {
            return;
        }

        ClientManager.OnGetServerMessage.AddListener(this.RecieveServerMessage);
        this.isIntialized = true;
    }

    private void RecieveServerMessage(ServerMessage message)
    {
        if (message.messageType != MessageType.KillLog)
        {
            return;
        }

        this.ShowKill(ulong.Parse(message.message), ulong.Parse(message.subMessage));
    }

    public void ShowKill(ulong murder, ulong killed)
    {
        GameObject log = Instantiate(this.killLogTabPrefab, this.logParent);
        log.GetComponent<KillLogTab>().Intialize(murder, killed);
    }

    private void OnDestroy()
    {
        if (this.isIntialized)
        {
            ClientManager.OnGetServerMessage.RemoveListener(this.RecieveServerMessage);
        }
    }
}
