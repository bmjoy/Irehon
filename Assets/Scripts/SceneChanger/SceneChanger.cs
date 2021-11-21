using Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    private Vector3 newPosition;
    [SerializeField]
    private string newScene;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("EntityBase"))
            return;

        Player player = other.GetComponent<Player>();
        if (player == null || !player.isServer)
            return;

        ChangeCharacterScene(player, newScene, newPosition);
    }

    public static void ChangeCharacterScene(Player player, string newScene, Vector3 newPosition)
    {
        PlayerConnectionInfo data = (PlayerConnectionInfo)player.connectionToClient.authenticationData;

        if (data.character.sceneChangeInfo != null)
            return;

        data.character.sceneChangeInfo = new SceneChangeInfo()
        {
            sceneName = newScene,
            spawnPosition = newPosition
        };

        ServerManager.Log(data.steamId, $"Sended self reconnect to change scene on {newScene}");
        Server.ServerManager.SendReconnectToThisServer(player.connectionToClient);
        player.connectionToClient.authenticationData = data;
        Server.ServerManager.WaitBeforeDisconnect(player.connectionToClient);
    }
}
