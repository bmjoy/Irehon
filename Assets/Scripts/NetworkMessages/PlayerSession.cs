using Mirror;
using UnityEngine;

namespace Irehon
{
    public struct PlayerSession : NetworkMessage
    {
        public ulong steamId;
        public bool isAuthorized;

        public AuthInfo authInfo;

        public CharacterInfo character;

        public bool isSpawnPointChanged;
        public string sceneToChange;

        public Transform playerPrefab;

        public PlayerSession(AuthInfo authInfo)
        {
            this.steamId = authInfo.Id;
            this.isAuthorized = false;
            this.playerPrefab = null;
            this.character = new CharacterInfo();
            this.authInfo = authInfo;

            this.isSpawnPointChanged = false;
            this.sceneToChange = null;
        }
    }
}