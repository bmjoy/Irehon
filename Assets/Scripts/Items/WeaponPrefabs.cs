using System;
using UnityEngine;

public class WeaponPrefabs : MonoBehaviour
{
    [SerializeField]
    private GameObject[] weaponPrefabs;

    private static WeaponPrefabs i;

    private void Awake()
    {
        if (i != null && i != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            i = this;
        }

        this.weaponPrefabs = Resources.LoadAll<GameObject>("Weapons");
    }

    public static GameObject GetWeaponPrefab(string slug)
    {
        return Array.Find(i.weaponPrefabs, x => x.name == slug);
    }
}
