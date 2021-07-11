using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Client : NetworkManager
{
    public UnityEvent OnUpdateCharacterList;
    private List<Character> charactersList = new List<Character>();

    public override void Start()
    {
        base.Start();
        if (OnUpdateCharacterList == null)
            OnUpdateCharacterList = new UnityEvent();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<Character>(SaveCharacter, true);
    }

    private void SaveCharacter(Character character)
    {
        if (!charactersList.Contains(character))
            charactersList.Add(character);
        OnUpdateCharacterList.Invoke();
    }

    public List<Character> GetCharacters() => charactersList;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        NetworkClient.PrepareToSpawnSceneObjects();
    }
}
