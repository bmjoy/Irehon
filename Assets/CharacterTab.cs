using UnityEngine;
using Client;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterTab : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Text nicknameField;

    private int slotId;
    private CharacterSelector selector;
    public void IntializeTab(CharacterSelector selector, Character character, int slot)
    {
        this.selector = selector;
        nicknameField.text = character.name;
        this.slotId = slot;
        GetComponent<Toggle>().group = selector.ToggleGroup;
    }

    public void Destroy()
    {
        selector.SelectCharacter(slotId);
        selector.CharacterOperationAction(CharacterOperations.Delete);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selector.SelectCharacter(slotId);
    }
}
