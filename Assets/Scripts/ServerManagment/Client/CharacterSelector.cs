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
    private GameObject playButton;
    [SerializeField]
    private Button[] characterSelections;
    [SerializeField]
    private Text nicknameField;
    [SerializeField]
    private RectTransform createCharacterTransform;
    [SerializeField]
    private Sprite activeCharacterSprite;
    private List<Character> characterList;
    private int selectedSlotId;

    private void Start()
    {
        characterList = NetworkManager.singleton.GetComponent<ClientManager>().GetCharacters();
        UpdateCharacterListUI();
        NetworkManager.singleton.GetComponent<ClientManager>().OnUpdateCharacterList.AddListener(UpdateCharacterListUI);
    }

    private void UpdateCharacterListUI() 
    {
        int slotId = 0;
        foreach (Character character in characterList)
            ShowCharacter(character, slotId++);
    }

    private void ShowCharacter(Character character, int slotId)
    {
        playButton.SetActive(true);
        if (slotId < maxCharacterSlots)
        {
            createCharacterTransform.gameObject.SetActive(true);
            createCharacterTransform.position = characterSelections[slotId + 1].GetComponent<RectTransform>().position;
        }
        else
            createCharacterTransform.gameObject.SetActive(false);
        characterSelections[slotId].gameObject.SetActive(true);
        characterSelections[slotId].GetComponent<Image>().sprite = activeCharacterSprite;
        characterSelections[slotId].GetComponentInChildren<Text>().text = character.NickName;
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
