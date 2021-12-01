using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ResourceEntity : LootableEntity
{
    [SerializeField, Tooltip("Object that contains mesh renderer for this mob")]
    private List<Renderer> modelParts;
    [SyncVar(hook = nameof(IsModelShownHook))]
    private bool isModelShown = true;

    protected override void Start()
    {
        base.Start();
        OnDeathEvent.AddListener(() => {
            foreach (var model in modelParts)
                model.enabled = false;
            if (isServer)
                isModelShown = false;
        });
        OnRespawnEvent.AddListener(() => {
            foreach (var model in modelParts)
                model.enabled = true;
            if (isServer)
                isModelShown = true;
        });
    }

    protected void IsModelShownHook(bool oldValue, bool newValue)
    {
        foreach (var model in modelParts)
            model.enabled = newValue;
    }
}
