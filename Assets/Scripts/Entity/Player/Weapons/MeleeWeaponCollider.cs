using Irehon.Entitys;
using Irehon.Entitys;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnNewCollectedEntityEvent : UnityEvent<Entity, EntityCollider> { }

public class MeleeWeaponCollider : MonoBehaviour
{
    [SerializeField]
    private List<EntityCollider> entityCollidersInZone = new List<EntityCollider>();
    [SerializeField]
    private List<EntityCollider> collectedCollidersInZone = new List<EntityCollider>();
    [SerializeField]
    private List<Collider> selfColliders;
    private bool isCollectingColliders;

    public OnNewCollectedEntityEvent OnNewCollectedEntityEvent { get; private set; } = new OnNewCollectedEntityEvent();
    private List<Entity> triggeredEntitys;

    public void Intialize(List<Collider> selfColliders)
    {
        this.selfColliders = selfColliders;
    }

    public Dictionary<Entity, EntityCollider> GetHittableEntities()
    {
        Dictionary<Entity, EntityCollider> entities = new Dictionary<Entity, EntityCollider>();
        foreach (EntityCollider collider in this.entityCollidersInZone)
        {
            if (!entities.ContainsKey(collider.GetParentEntityComponent()))
            {
                entities.Add(collider.GetParentEntityComponent(), collider);
            }
        }

        return entities;
    }

    public Dictionary<Entity, EntityCollider> GetCollectedInZoneEntities()
    {
        Dictionary<Entity, EntityCollider> entities = new Dictionary<Entity, EntityCollider>();
        foreach (EntityCollider collider in this.collectedCollidersInZone)
        {
            if (collider == null)
            {
                continue;
            }
            if (collider.GetParentEntityComponent() == null)
            {
                continue;
            }
            if (!entities.ContainsKey(collider.GetParentEntityComponent()))
            {
                entities.Add(collider.GetParentEntityComponent(), collider);
            }
        }
        return entities;
    }

    private void MeleeTrigger(EntityCollider entityCollider)
    {
        Entity entity = entityCollider.GetParentEntityComponent();

        if (this.triggeredEntitys.Contains(entity))
        {
            return;
        }

        this.triggeredEntitys.Add(entity);
        this.OnNewCollectedEntityEvent.Invoke(entity, entityCollider);
    }

    public void StartCollectColliders()
    {
        this.isCollectingColliders = true;
        this.triggeredEntitys = new List<Entity>();
        this.collectedCollidersInZone = new List<EntityCollider>(this.entityCollidersInZone);
        foreach (EntityCollider collider in this.collectedCollidersInZone)
        {
            this.MeleeTrigger(collider);
        }
    }

    public void StopCollectColliders()
    {
        this.isCollectingColliders = false;
        this.triggeredEntitys = new List<Entity>();
        this.collectedCollidersInZone = new List<EntityCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.selfColliders.Contains(other))
        {
            return;
        }

        if (other.CompareTag("Entity"))
        {
            EntityCollider collider = other.GetComponent<EntityCollider>();
            this.entityCollidersInZone.Add(collider);
            if (this.isCollectingColliders && !this.collectedCollidersInZone.Contains(collider))
            {
                this.collectedCollidersInZone.Add(collider);
                this.MeleeTrigger(collider);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (this.selfColliders.Contains(other))
        {
            return;
        }

        if (other.CompareTag("Entity"))
        {
            this.entityCollidersInZone.Remove(other.GetComponent<EntityCollider>());
        }
    }
}
