using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class ResourceEntity : LootableEntity
{
    [SerializeField, Tooltip("Object that contains mesh renderer for this mob")]
    private List<Renderer> modelParts;
    [SyncVar(hook = nameof(IsModelShownHook))]
    private bool isModelShown = true;

    protected override void Start()
    {
        base.Start();
        OnDeathEvent += (() =>
        {
            foreach (Renderer model in this.modelParts)
            {
                model.enabled = false;
            }

            if (this.isServer)
            {
                this.isModelShown = false;
            }
        });
        OnRespawnEvent += (() =>
        {
            foreach (Renderer model in this.modelParts)
            {
                model.enabled = true;
            }

            if (this.isServer)
            {
                this.isModelShown = true;
            }
        });
    }

    protected void IsModelShownHook(bool oldValue, bool newValue)
    {
        foreach (Renderer model in this.modelParts)
        {
            model.enabled = newValue;
        }
    }
}
