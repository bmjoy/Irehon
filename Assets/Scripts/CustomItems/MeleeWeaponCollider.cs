using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class OnMeleeWeaponTrigger : UnityEvent<Entity, EntityCollider> { }

public class MeleeWeaponCollider : MonoBehaviour
{
    private List<EntityCollider> entityCollidersInZone = new List<EntityCollider>();
    private List<EntityCollider> collectedCollidersInZone = new List<EntityCollider>();
    private List<Collider> selfColliders;
    private bool isCollectingColliders;

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
            if (!entities.ContainsKey(collider.GetParentEntityComponent()))
                entities.Add(collider.GetParentEntityComponent(), collider);
        return entities;
    }

    public void StartCollectColliders()
    {
        isCollectingColliders = true;
        collectedCollidersInZone = new List<EntityCollider>(entityCollidersInZone);
    }

    public void StopCollectColliders()
    {
        isCollectingColliders = false;
        collectedCollidersInZone.Clear();
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
                collectedCollidersInZone.Add(collider);
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
