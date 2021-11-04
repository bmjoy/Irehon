using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeleeWeaponCollider : MonoBehaviour
{
    private List<EntityCollider> entityCollidersInZone = new List<EntityCollider>();
    private List<Collider> selfColliders;

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


    private void OnTriggerEnter(Collider other)
    {
        if (selfColliders.Contains(other))
            return; 
        if (other.CompareTag("Entity"))
        {
            entityCollidersInZone.Add(other.GetComponent<EntityCollider>());
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
