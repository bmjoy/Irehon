using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceEntity : LootableEntity
{
    private Collider[] collisionColliders;
    private Renderer[] renderers;

    protected override void Awake()
    {
        base.Awake();
        collisionColliders = Array.FindAll(GetComponentsInChildren<Collider>(), c => !c.isTrigger);
        renderers = GetComponentsInChildren<Renderer>();

        IsAliveValueChanged += UpdateModel;
    }

    private void UpdateModel(bool isAlive)
    {
        foreach (var collider in collisionColliders)
            collider.isTrigger = !isAlive;

        foreach (var renderer in renderers)
            renderer.enabled = isAlive;
    }
}
