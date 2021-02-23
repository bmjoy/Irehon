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

public struct CharacterSelection : NetworkMessage
{
    public int selectedSlot;
}

public class CharacterSelector : MonoBehaviour
{
    [SerializeField]
    private Button[] characterSelections;
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
}
