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
    
    [SerializeField, Tooltip("Object that contains mesh renderer for this mob")]
    private GameObject model;
    protected MobStateMachine stateMachine;

    protected override void Start()
    {
        stateMachine = GetComponent<MobStateMachine>();
        OnDeathEvent.AddListener(() => stateMachine.SetNewState(new MobDeathState(this)));
        OnRespawnEvent.AddListener(() => stateMachine.SetNewState(new MobIdleState(this)));
        base.Start();
    }

    protected override void SetDefaultState()
    {
        isAlive = true;
        SetHealth(maxHealth);
        stateMachine.SetNewState(new MobIdleState(this));
    }
}
