using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class OnAgro : UnityEvent<Entity> { }

[RequireComponent(typeof(MobMovement), typeof(SphereCollider))]
public class AggressiveMob : Mob
{
    [HideInInspector]
    public Entity target;

    public OnAgro OnAgroEvent = new OnAgro();

    [Tooltip("При каком расстоянии моб может преследовать другого, пока не потеряет его из виду")]
    public float UnagroRadius = 8;
    [Tooltip("С какого радиуса от его спавна моба отагрит и вернет на его спавнпоинт")]
    public float AvoidRadius = 30;
    [Tooltip("Какая дистанция должна быть, чтоб начать атаковать, например если ближник то 1 метр (1)")]
    public float RequiredAttackDistance = 2;
    protected override void Start()
    {
        base.Start();
        OnAgroEvent.AddListener(x => stateMachine.SetNewState(new MobAgressiveWanderState(this)));
        OnAgroEvent.AddListener(entity =>
        {
            entity.OnDeathEvent.AddListener(UnAgro);
        });

        var collider = GetComponent<SphereCollider>();
        if (collider != null && collider.isTrigger)
        {
            if (collider.radius > UnagroRadius)
                UnagroRadius = collider.radius + 1;
        }
    }

    public void UnAgro()
    {
        target?.OnDeathEvent.RemoveListener(UnAgro);
        target = null;
        stateMachine.SetNewState(new MobIdleState(this));
    }

    public void Agro(Entity entity)
    {
        target = entity;
        OnAgroEvent.Invoke(entity);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isClient)
            return;
        if (target == null && isAlive && other.CompareTag("EntityBase"))
        {
            var entity = other.GetComponent<Entity>();

            if (!entity.isAlive)
                return;

            if (FractionBehaviourData != null)
            {
                if (FractionBehaviourData.Behaviours[entity.fraction] == FractionBehaviour.Agressive)
                    Agro(entity);
            }
            else
                Agro(entity);

        }
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        if (!isAlive)
            return;
        Agro(damageMessage.source);
        base.TakeDamage(damageMessage);
    }

    public override void SetDefaultState()
    {
        base.SetDefaultState();
        UnAgro();
    }
}

