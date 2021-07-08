using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract AbilityBase Setup(AbilitySystem abilitySystem);
}
