using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityTreeController : MonoBehaviour, IAbilityTreeController
{
    public static IAbilityTreeController instance;

    [SerializeField]
    private Canvas abilityTreeCanvas;
    [SerializeField]
    private TextMeshProUGUI abilityTitle;
    [SerializeField]
    private TextMeshProUGUI abilityDescribe;
    [SerializeField]
    private GameObject upgradeButton;
    [SerializeField]
    private RectTransform abilityDragger;

    private int selectedAbilityId;
    private IAbility selectedAbility;
    private List<IAbility> abilitys;
    private List<IAbilityTreeSlot> treeSlots;
    private List<int> unlockedSkills;


    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
        treeSlots = new List<IAbilityTreeSlot>(abilityTreeCanvas.GetComponentsInChildren<IAbilityTreeSlot>(true));
    }

    public void OnSelectSkill(int id)
    {
        selectedAbility = abilitys.Find(p => p.Id == id);
        if (selectedAbility == null)
        {
            Debug.Log("Ability with " + id + " not found to select");
            return;
        }

        selectedAbilityId = id;

        if (!unlockedSkills.Contains(id))
            upgradeButton.SetActive(true);
        else
            upgradeButton.SetActive(false);

        abilityTitle.text = selectedAbility.Title;
        abilityDescribe.text = selectedAbility.Describe;
    }

    public void UnlockSelectedSkill()
    {

    }

    public void SetUnlockedSkillsInfo(List<int> unlockedSkills)
    {
        this.unlockedSkills = unlockedSkills;
        foreach(AbilityTreeSlot treeSlot in treeSlots)
        {
            if (unlockedSkills.Contains(treeSlot.SkillId))
                treeSlot.SetState(true);
            else
                treeSlot.SetState(false);
        }
    }

    public RectTransform GetDragger() => abilityDragger;

    public Canvas GetSlotCanvas() => abilityTreeCanvas;

    public void SetAbilitys(List<IAbility> abilities)
    {
        abilitys = abilities;
        foreach(AbilityTreeSlot treeSlot in treeSlots)
        {
            treeSlot.SetAbilityInfo(abilities.Find(x => x.Id == treeSlot.SkillId));
        }
    }
}
