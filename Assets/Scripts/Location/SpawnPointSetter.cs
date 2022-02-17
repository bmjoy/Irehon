using Irehon;
using UnityEngine;

public class SpawnPointSetter : MonoBehaviour
{
    [SerializeField]
    private Vector3 newSpawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("EntityBase"))
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        if (player == null || !player.isServer)
        {
            return;
        }

        PlayerSession data = (PlayerSession)player.connectionToClient.authenticationData;

        data.character.spawnPoint = this.newSpawnPoint;
        data.character.spawnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        player.connectionToClient.authenticationData = data;
    }
}
