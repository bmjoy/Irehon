using Mirror;
using UnityEngine;

public struct PlayerConnectionInfo : NetworkMessage
{
    public ulong steamId;
    public bool isAuthorized;

    public AuthRequestMessage authInfo;

    public CharacterInfo character;

    public bool isSpawnPointChanged;
    public string sceneToChange;

    public Transform playerPrefab;

    public PlayerConnectionInfo(AuthRequestMessage authInfo)
    {
        this.steamId = authInfo.Id;
        isAuthorized = false;
        playerPrefab = null;
        character = new CharacterInfo();
        this.authInfo = authInfo;

        isSpawnPointChanged = false;
        sceneToChange = null;
    }
}
