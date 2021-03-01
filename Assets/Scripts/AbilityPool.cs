using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPool : MonoBehaviour
{
    private AbilityPool instance;

    [SerializeField]
    private AbilityBase abilitys;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        gameObject.AddComponent(Type.GetType("AbilitySlot"));
    }
}
