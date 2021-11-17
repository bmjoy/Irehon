using UnityEngine;

public class EntityCollider : MonoBehaviour
{
    public float damageMultiplier = 1;

    [SerializeField]
    private Entity parent;

    public Entity GetParentEntityComponent()
    {
        return parent;
    }
}
