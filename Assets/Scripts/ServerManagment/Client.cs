using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Client : NetworkManager
{
    private List<Character> charactersList = new List<Character>();
    public UnityEvent OnUpdateCharacterList;

    public override void Start()
    {
        base.Start();
        if (OnUpdateCharacterList == null)
            OnUpdateCharacterList = new UnityEvent();
        NetworkClient.RegisterHandler<Character>(SaveCharacter, false);
    }

    private void SaveCharacter(Character character)
    {
        if (!charactersList.Contains(character))
            charactersList.Add(character);
        OnUpdateCharacterList.Invoke();
    }

    public List<Character> GetCharacters() => charactersList;


    public override void OnClientConnect(NetworkConnection conn)
    {
        print("connected");
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        print("change to " + newSceneName);
    }
}
