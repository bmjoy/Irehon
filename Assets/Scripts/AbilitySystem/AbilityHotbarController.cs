using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityHotbarController : MonoBehaviour, IAbilityHotbarController
{
    private List<IAbilitySlot> slotPool;

    [SerializeField]
    private Canvas abilitySlotsCanvas;

    public static IAbilityHotbarController instance;

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;
    }

    private void Start()
    {
        slotPool = new List<IAbilitySlot>(abilitySlotsCanvas.GetComponentsInChildren<IAbilitySlot>());
    }

    public IAbility GetAbilityFromSlot(int slotId)
    {
        if (slotId < 0 || slotId > slotPool.Count)
        {
            Debug.Log("Slot out of rength");
            return null;
        }
        return slotPool[slotId].CurrentAbility;
    }

    public void SetAbilitySystem(IAbilitySystem abilitySystem)
    {
        int i = 0;
        foreach (IAbilitySlot abilitySlot in slotPool)
        {
            abilitySlot.Intialize(i, abilitySystem);
            i++;
        }
    }

    public void UnSetAbilityFromSlot(int id)
    {
        if (id < 0 || id > slotPool.Count)
        {
            Debug.Log("Slot out of rength");
            return;
        }
        slotPool[id].UnSetCurrentAbility();
    }

    public void UnSetAbilityFromSlot(IAbility ability)
    {
        IAbilitySlot removingSlot = slotPool.Find(p => p.CurrentAbility == ability);
        if (removingSlot == null)
        {
            Debug.Log("Slot to unset with id " + ability.Id + " not found");
            return;
        }
        removingSlot.UnSetCurrentAbility();
    }

    public void SetAbilityOnSlot(IAbility ability, int slot)
    {
        if (slot < 0 || slot > slotPool.Count)
        {
            Debug.Log("Slot out of rength");
            return;
        }
        slotPool[slot].SetAbility(ability);
    }

    public void InvokeGlobalCooldown(float time)
    {
        foreach (AbilitySlot slot in slotPool)
            slot.GlobalCooldown(time);
    }
}
