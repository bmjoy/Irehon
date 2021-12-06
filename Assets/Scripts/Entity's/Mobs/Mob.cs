using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.Events;

public class OnDestinationChangeEvent : UnityEvent<Vector3> { }

[RequireComponent(typeof(NavMeshAgent), typeof(MobStateMachine))]
public class Mob : LootableEntity
{
    public List<Renderer> ModelParts => modelParts;

    [SyncVar(hook = nameof(IsModelShownHook))]
    private bool isModelShown = true;

    [SerializeField, Tooltip("Object that contains mesh renderer for this mob")]
    private List<Renderer> modelParts;
    protected MobStateMachine stateMachine;

    protected override void Start()
    {
        stateMachine = GetComponent<MobStateMachine>();
        OnDeathEvent.AddListener(() => {
            foreach (var model in ModelParts)
                model.enabled = false;
                });
        OnRespawnEvent.AddListener(() => {
            foreach (var model in ModelParts)
                model.enabled = true;
        });
        if (isServer)
        {
            OnDeathEvent.AddListener(() => stateMachine.SetNewState(new MobDeathState(this)));
            OnRespawnEvent.AddListener(() => stateMachine.SetNewState(new MobIdleState(this)));
        }
        base.Start();
    }

    private void OnEnable()
    {
        foreach (var model in ModelParts)
            model.enabled = isModelShown;
    }

    protected void IsModelShownHook(bool oldValue, bool newValue)
    {
        foreach (var model in ModelParts)
            model.enabled = newValue;
    }

    public void ChangeModelState(bool newState)
    {
        isModelShown = newState;
    }

    public override void SetDefaultState()
    {
        isAlive = true;
        SetHealth(maxHealth);
        GetComponent<NavMeshAgent>().Warp(startPosition);
        stateMachine.SetNewState(new MobIdleState(this));
    }
}
