using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerClass
{
    [SerializeField]
    private GameObject[] armorParts;
    [SerializeField]
    private ClassData dataClass;

    private Transform playerTransform;

    public void DisableArmorParts()
    {
        foreach (GameObject armorPart in armorParts)
            armorPart.SetActive(false);
    }

    public void EnableArmorParts(Transform player)
    {
        playerTransform = player;
        foreach (GameObject armorPart in armorParts)
            armorPart.SetActive(true);
    }

    public void RMBDown()
    {
        dataClass.RightMouseButtonDown(playerTransform);
    }

    public void RMBUp()
    {
        dataClass.RightMouseButtonUp(playerTransform);
    }

    public void LMBDown()
    {
        dataClass.LeftMouseButtonDown(playerTransform);
    }

    public void LMBUp()
    {
        dataClass.LeftMouseButtonUp(playerTransform);
    }

    public string GetClassName()
    {
        return dataClass.ClassName;
    }

    public void SetAnimationOverride(Animator animator)
    {
        animator.runtimeAnimatorController = dataClass.classAnimator;
    }
}
