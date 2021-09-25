using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPrefabs : MonoBehaviour
{
    [SerializeField]
    private GameObject[] weaponPrefabs;

    private static WeaponPrefabs i;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(gameObject);
        else
            i = this;

        weaponPrefabs = Resources.LoadAll<GameObject>("Weapons");
    }

    public static GameObject GetWeaponPrefab(string slug) => Array.Find(i.weaponPrefabs, x => x.name == slug);
}
