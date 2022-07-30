using Irehon.Entitys;
using UnityEngine;
using UnityEngine.Events;

public class OnAgro : UnityEvent<Entity> { }

[RequireComponent(typeof(MobMovement), typeof(SphereCollider))]
public class AggressiveMob : Mob
{
    [HideInInspector]
    public Entity target;

    public OnAgro OnAgroEvent = new OnAgro();

    [HideInInspector]
    public float UnagroRadius;
    [HideInInspector]
    public float AvoidRadius;
    [Tooltip("Какая дистанция должна быть, чтоб начать атаковать, например если ближник то 1 метр (1)")]
    public float RequiredAttackDistance = 2;
    protected override void Start()
    {
        base.Start();
        this.OnAgroEvent.AddListener(x => this.stateMachine.SetNewState(new MobAgressiveWanderState(this)));
        this.OnAgroEvent.AddListener(entity =>
        {
            entity.Dead += this.UnAgro;
        });

        SphereCollider collider = this.GetComponent<SphereCollider>();
        if (collider != null && collider.isTrigger)
        {
            UnagroRadius = collider.radius + 10;
            AvoidRadius = UnagroRadius * 2;
        }
    }

    public virtual void UnAgro()
    {
        ResetAgro();
        this.stateMachine.SetNewState(new MobIdleState(this));
    }

    public void ResetAgro()
    {
        if (this.target != null)
        {
            this.target.Dead -= this.UnAgro;
        }

        this.target = null;
    }
    public void Agro(Entity entity)
    {
        if (!entity.isAlive || !stateMachine.CurrentState.CanAgro)
            return;

        this.target = entity;
        this.OnAgroEvent.Invoke(entity);
    }

    private void OnTriggerStay(Collider other)
    {
        if (this.isClient)
        {
            return;
        }

        if (this.target == null && this.isAlive && other.CompareTag("EntityBase"))
        {
            Entity entity = other.GetComponent<Entity>();

            if (!entity.isAlive)
            {
                return;
            }

            if (this.FractionBehaviourData != null)
            {
                if (this.FractionBehaviourData.Behaviours[entity.fraction] == FractionBehaviour.Agressive)
                {
                    this.Agro(entity);
                }
            }
            else
            {
                this.Agro(entity);
            }
        }
    }

    public override void SetDefaultState()
    {
        base.SetDefaultState();
        this.UnAgro();
    }
}

