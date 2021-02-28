using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUIController : MonoBehaviour
{
    [SerializeField]
    private AbilitySlot[] slotPool;

    public static AbilityUIController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    public void SetAbilityOnSlot(IAbility ability, int slot)
    {
        if (slot < 0 || slot > slotPool.Length)
            Debug.Log("Slot out of rength");
        slotPool[slot].SetAbility(ability);
    }

    public void InvokeGlobalCooldown(float time)
    {
        foreach (AbilitySlot slot in slotPool)
            slot.GlobalCooldown(time);
    }
}
