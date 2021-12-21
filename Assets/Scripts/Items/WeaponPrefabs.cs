using System;
using UnityEngine;

public static class WeaponPrefabs
{
    static WeaponPrefabs()
    {
        weapons = Resources.LoadAll<GameObject>("Weapons");
    }

    private static GameObject[] weapons;

    public static GameObject GetWeaponPrefab(string slug)
    {
        return Array.Find(weapons, x => x.name == slug);
    }
}
