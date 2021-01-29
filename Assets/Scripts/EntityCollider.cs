using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCollider : MonoBehaviour
{
    [SerializeField]
    private Entity parent;

    public Entity GetParentEntityComponent()
    {
        return parent;
    }
}
