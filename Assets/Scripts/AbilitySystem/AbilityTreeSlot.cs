using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AbilityTreeSlot : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IAbilityTreeSlot, IPointerClickHandler
{
    public int SkillId => skillId;

    [SerializeField]
    private int skillId;
    [SerializeField]
    private Image imageSlot;
    [SerializeField]
    private TextMeshProUGUI requiredLvl;
    private Canvas canvas;

    public void OnPointerClick(PointerEventData data)
    {
        AbilityTreeController.instance.OnSelectSkill(skillId);
    }


    public void OnBeginDrag(PointerEventData data)
    {
        AbilityTreeController.instance.GetDragger().gameObject.SetActive(true);
        AbilityTreeController.instance.GetDragger().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        Image dragImage = AbilityTreeController.instance.GetDragger().GetComponent<Image>();
        dragImage.sprite = imageSlot.sprite;
    }

    public void OnDrag(PointerEventData data)
    {
        AbilityTreeController.instance.GetDragger().anchoredPosition += data.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData data)
    {
        AbilityTreeController.instance.GetDragger().gameObject.SetActive(false);
    }

    public void SetAbilityInfo(IAbility ability)
    {
        if (ability == null)
        {
            Debug.Log("Ability with " + skillId + " not found");
            return;
        }
        imageSlot.sprite = ability.AbilityIcon;
        requiredLvl.text = ability.UnlockRequirment.RequiredSkillLvl.ToString();
        canvas = AbilityTreeController.instance.GetSlotCanvas();
    }

    public void SetState(bool state)
    {
        if (state)
            imageSlot.color = Color.white;
        else
            imageSlot.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }
}
