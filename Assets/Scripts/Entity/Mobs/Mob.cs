using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class OnDestinationChangeEvent : UnityEvent<Vector3> { }

[RequireComponent(typeof(NavMeshAgent), typeof(MobStateMachine))]
public class Mob : LootableEntity
{
    public List<Renderer> ModelParts => this.modelParts;

    [SyncVar(hook = nameof(IsModelShownHook))]
    private bool isModelShown = true;

    [SerializeField, Tooltip("Object that contains mesh renderer for this mob")]
    private List<Renderer> modelParts;
    protected MobStateMachine stateMachine;

    protected override void Start()
    {
        this.stateMachine = this.GetComponent<MobStateMachine>();
        OnDeathEvent += () =>
        {
            foreach (Renderer model in this.ModelParts)
            {
                model.enabled = false;
            }
        };
        OnRespawnEvent += () =>
        {
            foreach (Renderer model in this.ModelParts)
            {
                model.enabled = true;
            }
        };
        if (this.isServer)
        {
            OnDeathEvent += () => this.stateMachine.SetNewState(new MobDeathState(this));
            OnRespawnEvent += () => this.stateMachine.SetNewState(new MobIdleState(this));
        }
        base.Start();
    }

    private void OnEnable()
    {
        foreach (Renderer model in this.ModelParts)
        {
            model.enabled = this.isModelShown;
        }
    }

    protected void IsModelShownHook(bool oldValue, bool newValue)
    {
        foreach (Renderer model in this.ModelParts)
        {
            model.enabled = newValue;
        }
    }

    public void ChangeModelState(bool newState)
    {
        this.isModelShown = newState;
    }

    public override void SetDefaultState()
    {
        this.isAlive = true;
        this.SetHealth(this.maxHealth);
        this.GetComponent<NavMeshAgent>().Warp(this.startPosition);
        this.stateMachine.SetNewState(new MobIdleState(this));
    }
}
