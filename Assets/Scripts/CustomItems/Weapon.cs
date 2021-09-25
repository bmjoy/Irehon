using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Fist, Bow };
public abstract class Weapon : MonoBehaviour
{
    new public abstract WeaponType GetType();
    public abstract AbilityBase Setup(AbilitySystem abilitySystem);
    public abstract void UnSetup(AbilitySystem abilitySystem);
}
