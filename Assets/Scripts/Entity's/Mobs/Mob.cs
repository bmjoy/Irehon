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
    public GameObject Model => model;

    [SyncVar(hook = nameof(IsModelShownHook))]
    private bool isModelShown = true;

    [SerializeField, Tooltip("Object that contains mesh renderer for this mob")]
    private GameObject model;
    protected MobStateMachine stateMachine;

    protected override void Start()
    {
        stateMachine = GetComponent<MobStateMachine>();
        if (isServer)
        {
            OnDeathEvent.AddListener(() => stateMachine.SetNewState(new MobDeathState(this)));
            OnRespawnEvent.AddListener(() => stateMachine.SetNewState(new MobIdleState(this)));
        }
        base.Start();
    }

    protected void IsModelShownHook(bool oldValue, bool newValue)
    {
        model.SetActive(newValue);
    }

    public void ChangeModelState(bool newState)
    {
        isModelShown = newState;
    }

    protected override void SetDefaultState()
    {
        isAlive = true;
        SetHealth(maxHealth);
        GetComponent<NavMeshAgent>().Warp(spawnPosition);
        stateMachine.SetNewState(new MobIdleState(this));
    }
}
