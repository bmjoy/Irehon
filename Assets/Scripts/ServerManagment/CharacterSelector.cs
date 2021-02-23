using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public struct Character : NetworkMessage
{
    public enum CharacterClass { Archer };
    public int slot;
    public CharacterClass Class;
    public string NickName;
    public Vector3 position;
}

public struct CharacterCreate : NetworkMessage
{
    public string NickName;
    public Character.CharacterClass Class;
}

public struct CharacterSelection : NetworkMessage
{
    public int selectedSlot;
}

public class CharacterSelector : MonoBehaviour
{
    int maxCharacterSlots = 3;
    [SerializeField]
    private Button[] characterSelections;
    [SerializeField]
    private Text nicknameField;
    [SerializeField]
    private Button createCharacterButton;
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
        if (slotId != maxCharacterSlots)
            createCharacterButton.gameObject.SetActive(true);
        else
            createCharacterButton.gameObject.SetActive(false);
        characterSelections[slotId].gameObject.SetActive(true);
        characterSelections[slotId].GetComponentInChildren<Text>().text = 
            character.NickName + "\n" + character.Class.ToString();
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
            Class = Character.CharacterClass.Archer,
            NickName = nicknameField.text
        };
        NetworkClient.Send(createQuerry);
    }
}
