using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour, IDropHandler, IAbilitySlot
{
    [SerializeField]
    private Text triggerKey;
    [SerializeField]
    private Image cooldownImage;
    [SerializeField]
    private Image abilityIcon;

    private Sprite defaultSprite;

    public IAbility CurrentAbility => currentAbility;

    private IAbility currentAbility;
    private IAbilitySystem currentAbilitySystem;

    private float currentCooldown;
    private int slotId;

    private Coroutine cooldownCoroutine;

    private void Start()
    {
        defaultSprite = abilityIcon.sprite;
    }

    public void OnDrop(PointerEventData data)
    {
        if (currentAbilitySystem == null)
        {
            Debug.Log("Ability system null");
            return;
        }
        AbilityTreeSlot sourceSlot = data.pointerDrag.GetComponent<AbilityTreeSlot>();
        currentAbilitySystem.TrySetAbilityToSlot(sourceSlot.SkillId, slotId);
    }

    public void Intialize(int slotId, IAbilitySystem abilitySystem)
    {
        print("intialized");
        this.slotId = slotId;
        currentAbilitySystem = abilitySystem;
    }

    public void SetAbility(IAbility ability) 
    {
        triggerKey.text = ability.TriggerKey.ToString();
        abilityIcon.sprite = ability.AbilityIcon;
        if (currentAbility != null)
            currentAbility.OnAbilityCooldown.RemoveListener(StartCooldown);
        ability.OnAbilityCooldown.AddListener(StartCooldown);
        currentAbility = ability;
    }

    public void UnSetCurrentAbility()
    {
        if (currentAbility == null)
        {
            Debug.Log("Ability already null");
            return;
        }
        triggerKey.text = "";
        abilityIcon.sprite = defaultSprite;
        currentAbility.OnAbilityCooldown.RemoveListener(StartCooldown);
        currentAbility = null;
    }

    public void StartCooldown(float cooldownTime)
    {
        currentCooldown = cooldownTime;
        float requiredTime = cooldownTime;
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(SliderCooldown());
        IEnumerator SliderCooldown()
        {
            while (currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
                cooldownImage.fillAmount = currentCooldown / requiredTime;
                yield return null;
            }
        }
    }

    public void GlobalCooldown(float time)
    {
        if (currentCooldown < time)
            StartCooldown(time);
    }
}
