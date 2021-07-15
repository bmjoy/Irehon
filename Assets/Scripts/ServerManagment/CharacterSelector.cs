using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public struct Character : NetworkMessage
{
    public int slot;
    public int id;
    public string NickName;
    public Vector3 position;
}

public struct CharacterCreate : NetworkMessage
{
    public string NickName;
}

public struct CharacterSelection : NetworkMessage
{
    public int selectedSlot;
}

public class CharacterSelector : MonoBehaviour
{
    int maxCharacterSlots = 2;
    [SerializeField]
    private Button[] characterSelections;
    [SerializeField]
    private Text nicknameField;
    [SerializeField]
    private RectTransform createCharacterTransform;
    private List<Character> characterList;
    private int selectedSlotId;

    private void Start()
    {
        characterList = NetworkManager.singleton.GetComponent<Client>().GetCharacters();
        UpdateCharacterListUI();
        NetworkManager.singleton.GetComponent<Client>().OnUpdateCharacterList.AddListener(UpdateCharacterListUI);
    }

    private void UpdateCharacterListUI()
    {
        int slotId = 0;
        foreach (Character character in characterList)
            ShowCharacter(character, slotId++);
    }

    private void ShowCharacter(Character character, int slotId)
    {
        if (slotId < maxCharacterSlots)
            createCharacterTransform.gameObject.SetActive(true);
        else
            createCharacterTransform.gameObject.SetActive(false);
        characterSelections[slotId].gameObject.SetActive(true);
        characterSelections[slotId].GetComponentInChildren<Text>().text =
            character.NickName;
    }

    public void SelectCharacter(int slotId) => selectedSlotId = slotId;

    public void PlayButton() => Play();

    private void Play()
    {
        NetworkClient.Send(new CharacterSelection { selectedSlot = selectedSlotId });
    }

    public void CreateCharacterButton()
    {
        CharacterCreate createQuerry = new CharacterCreate
        {
            NickName = nicknameField.text
        };
        NetworkClient.Send(createQuerry);
    }
}
