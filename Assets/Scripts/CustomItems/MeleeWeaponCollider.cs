using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
        foreach (var collider in entityCollidersInZone)
            if (!entities.ContainsKey(collider.GetParentEntityComponent()))
                entities.Add(collider.GetParentEntityComponent(), collider);
        return entities;
    }

    public Dictionary<Entity, EntityCollider> GetCollectedInZoneEntities()
    {
        Dictionary<Entity, EntityCollider> entities = new Dictionary<Entity, EntityCollider>();
        foreach (var collider in collectedCollidersInZone)
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
                entities.Add(collider.GetParentEntityComponent(), collider);
        }
        return entities;
    }

    private void MeleeTrigger(EntityCollider entityCollider)
    {
        var entity = entityCollider.GetParentEntityComponent();

        if (triggeredEntitys.Contains(entity))
            return;

        triggeredEntitys.Add(entity);
        OnNewCollectedEntityEvent.Invoke(entity, entityCollider);
    }

    public void StartCollectColliders()
    {
        isCollectingColliders = true;
        triggeredEntitys = new List<Entity>();
        collectedCollidersInZone = new List<EntityCollider>(entityCollidersInZone);
        foreach (EntityCollider collider in collectedCollidersInZone)
            MeleeTrigger(collider);
    }

    public void StopCollectColliders()
    {
        isCollectingColliders = false;
        triggeredEntitys = new List<Entity>();
        collectedCollidersInZone = new List<EntityCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (selfColliders.Contains(other))
            return; 
        if (other.CompareTag("Entity"))
        {
            var collider = other.GetComponent<EntityCollider>();
            entityCollidersInZone.Add(collider);
            if (isCollectingColliders && !collectedCollidersInZone.Contains(collider))
            {
                collectedCollidersInZone.Add(collider);
                MeleeTrigger(collider);
            }
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (selfColliders.Contains(other))
            return;
        if (other.CompareTag("Entity"))
        {
            entityCollidersInZone.Remove(other.GetComponent<EntityCollider>());
        }
    }
}
