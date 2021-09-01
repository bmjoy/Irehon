using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Client;
using SimpleJSON;
using UnityEngine.EventSystems;

public struct Character
{
    public int id;
    public string name;

    public Character(JSONNode node)
    {
        id = node["id"].AsInt;
        name = node["name"].Value;
    }
}

namespace Client
{
    public enum CharacterOperations { Play, Create, Delete }
    public struct CharacterOperationRequest : NetworkMessage
    {
        public CharacterOperations opeartion;
        public string nickname;
        public int selectedSlot;
    }

    public class CharacterSelector : MonoBehaviour
    {
        [SerializeField]
        private Text nicknameField;
        [SerializeField]
        private RectTransform createCharacterTransform;
        [SerializeField]
        private GameObject characterPrefab;
        [SerializeField]
        private ToggleGroup toggleGroup;

        public ToggleGroup ToggleGroup => toggleGroup;

        private List<Character> characterList;
        private List<GameObject> createdCharacterTabs = new List<GameObject>();
        private int selectedSlotId;

        private void Start()
        {
            characterList = NetworkManager.singleton.GetComponent<ClientManager>().GetCharacters();
            UpdateCharacterListUI(characterList);
            NetworkManager.singleton.GetComponent<ClientManager>().OnUpdateCharacterList.AddListener(UpdateCharacterListUI);
        }

        private CharacterOperationRequest CreateCharacterRequest(CharacterOperations operation)
        {
            switch (operation)
            {
                case CharacterOperations.Create:
                    {
                        var request = new CharacterOperationRequest();
                        request.opeartion = operation;
                        request.nickname = nicknameField.text;
                        return request;
                    }
                case CharacterOperations.Delete:
                    {
                        var request = new CharacterOperationRequest();
                        request.opeartion = operation;
                        request.selectedSlot = selectedSlotId;
                        return request;
                    }
                case CharacterOperations.Play:
                    {
                        var request = new CharacterOperationRequest();
                        request.opeartion = operation;
                        request.selectedSlot = selectedSlotId;
                        return request;
                    }
                default:
                    throw new System.Exception("Character operation request error: unknown type");
            }
        }

        private void UpdateCharacterListUI(List<Character> characterList)
        {
            foreach (GameObject tab in createdCharacterTabs)
                Destroy(tab);

            createdCharacterTabs.Clear();

            int slotId = 0;

            foreach (Character character in characterList)
                createdCharacterTabs.Add(CreateCharacterTab(character, slotId++));

            if (createdCharacterTabs.Count > 0)
                selectedSlotId = 0;
        }

        private GameObject CreateCharacterTab(Character character, int slotId)
        {
            GameObject newTab = Instantiate(characterPrefab, createCharacterTransform);

            newTab.GetComponent<CharacterTab>().IntializeTab(this, character, slotId);

            return newTab;
        }

        public void SelectCharacter(int slotId)=> selectedSlotId = slotId;

        public void CreateCharacterButton() => CharacterOperationAction(CharacterOperations.Create);

        public void PlayButton() => CharacterOperationAction(CharacterOperations.Play);

        public void CharacterOperationAction(CharacterOperations operation)
        {
            NetworkClient.Send(CreateCharacterRequest(operation));
        }
    }
}