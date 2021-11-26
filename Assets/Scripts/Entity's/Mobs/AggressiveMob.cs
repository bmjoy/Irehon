using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AggressiveMob : Mob
{
    public Entity target;

    [Tooltip("При каком расстоянии моб может преследовать другого, пока не потеряет его из виду")]
    public float UnagroRadius = 8;
    [Tooltip("С какого радиуса от его спавна моба отагрит и вернет на его спавнпоинт")]
    public float AvoidRadius = 30;
    [Tooltip("Какая дистанция должна быть, чтоб начать атаковать, например если ближник то 1 метр (1)")]
    public float RequiredAttackDistance = 2;
    protected override void Start()
    {
        base.Start();
    }
    private void OnTriggerStay(Collider other)
    {
        if (isClient)
            return;
        if (target == null && isAlive && other.CompareTag("EntityBase"))
        {
            var entity = other.GetComponent<Entity>();
            if (FractionBehaviourData.Behaviours[entity.fraction] == FractionBehaviour.Agressive)
            {
                target = entity;
                stateMachine.SetNewState(new MobAgressiveWanderState(this));
            }
        }
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        if (!isAlive)
            return;
        target = damageMessage.source;
        stateMachine.SetNewState(new MobAgressiveWanderState(this));
        base.TakeDamage(damageMessage);
    }

    public override void SetDefaultState()
    {
        base.SetDefaultState();
        target = null;
    }
}

