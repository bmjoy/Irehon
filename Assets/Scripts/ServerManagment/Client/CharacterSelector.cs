using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Client;

public struct Character : NetworkMessage
{
    public int slot;
    public int id;
    public string name;
    public Vector3 position;
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

        private List<Character> characterList;
        private List<GameObject> createdCharacterTabs = new List<GameObject>();
        private int selectedSlotId;

        private void Start()
        {
            characterList = NetworkManager.singleton.GetComponent<ClientManager>().GetCharacters();
            UpdateCharacterListUI();
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

        private void UpdateCharacterListUI()
        {
            foreach (GameObject tab in createdCharacterTabs)
                Destroy(tab);

            int slotId = 0;

            foreach (Character character in characterList)
                createdCharacterTabs.Add(CreateCharacterTab(character, slotId++));
            if (createdCharacterTabs.Count > 0)
            {
                createdCharacterTabs[0].GetComponent<Toggle>().isOn = true;
                selectedSlotId = 0;
            }
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
            NetworkClient.Send(CreateCharacterRequest(CharacterOperations.Play));
        }
    }
}