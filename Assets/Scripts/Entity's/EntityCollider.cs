using UnityEngine;

public class EntityCollider : MonoBehaviour
{
    public float damageMultiplier;

    [SerializeField]
    private Entity parent;

    public Entity GetParentEntityComponent()
    {
        return parent;
    }
}
