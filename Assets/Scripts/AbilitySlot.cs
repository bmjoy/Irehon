using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour
{
    [SerializeField]
    private Text triggerKey;
    [SerializeField]
    private Slider cooldownSlider;
    [SerializeField]
    private Image abilityIcon;

    private IAbility currentAbility;

    private float currentCooldown;

    private Coroutine cooldownCoroutine;

    public void SetAbility(IAbility ability) 
    {
        triggerKey.text = ability.TriggerKey.ToString();
        abilityIcon.sprite = ability.AbilityIcon;
        if (currentAbility != null)
            currentAbility.OnAbilityCooldown.RemoveListener(StartCooldown);
        ability.OnAbilityCooldown.AddListener(StartCooldown);
        currentAbility = ability;
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
                cooldownSlider.value = currentCooldown / requiredTime;
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
