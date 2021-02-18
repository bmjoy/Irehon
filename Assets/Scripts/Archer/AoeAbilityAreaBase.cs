using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoeAbilityAreaBase : MonoBehaviour
{
    private List<Entity> entityOnArea = new List<Entity>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EntityBase"))
        {
            Entity entity = other.GetComponent<Entity>();
            if (IsApproachToAbility(entity) && !entityOnArea.Contains(entity))
            {
                OnAddToArea(entity);
                entityOnArea.Add(entity);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EntityBase"))
        {
            Entity entity = other.GetComponent<Entity>();
            if (IsApproachToAbility(entity) && entityOnArea.Contains(entity))
            {
                OnRemoveFromArea(entity);
                entityOnArea.Remove(entity);
            }
        }
    }

    public void TriggerEffectToAllInArea()
    {
        foreach (Entity entity in entityOnArea)
            Effect(entity);
    }

    protected abstract void Effect(Entity entity);

    protected abstract void OnRemoveFromArea(Entity entity);

    protected abstract void OnAddToArea(Entity entity);

    protected virtual bool IsApproachToAbility(Entity entity)
    {
        if (entity.IsAlive())
            return true;
        else
            return false;
    }
}
