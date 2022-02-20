using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class OnDestinationChangeEvent : UnityEvent<Vector3> { }

[RequireComponent(typeof(NavMeshAgent), typeof(MobStateMachine))]
public class Mob : LootableEntity
{
    protected MobStateMachine stateMachine;
    private Collider[] collisionColliders;
    private Renderer[] renderers;

    protected override void Awake()
    {
        base.Awake();
        collisionColliders = Array.FindAll(GetComponentsInChildren<Collider>(), c => !c.isTrigger);
        renderers = Array.FindAll(GetComponentsInChildren<Renderer>(), renderer => renderer.enabled);

        IsAliveValueChanged += UpdateModel;
    }

    protected override void Start()
    {
        this.stateMachine = this.GetComponent<MobStateMachine>();

        if (this.isServer)
        {
            Dead += () => this.stateMachine.SetNewState(new MobDeathState(this));
            Respawned += () => this.stateMachine.SetNewState(new MobIdleState(this));
        }
        base.Start();
    }

    private void UpdateModel(bool isAlive)
    {
        foreach (var collider in collisionColliders)
            collider.isTrigger = !isAlive;

        foreach (var renderer in renderers)
            renderer.enabled = isAlive;
    }

    public override void SetDefaultState()
    {
        if (isServer)
            this.isAlive = true;
        IsAliveHook(isAlive, isAlive);
        this.SetHealth(this.maxHealth);
        this.GetComponent<NavMeshAgent>().Warp(this.startPosition);
        this.stateMachine.SetNewState(new MobIdleState(this));
    }

    public void SetMaxHealth()
    {
        this.SetHealth(this.maxHealth);
    }

}
