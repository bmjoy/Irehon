using UnityEngine;

public class EntityCollider : MonoBehaviour
{
    public float damageMultiplier = 1;

    [SerializeField]
    private Entity parent;

    private void Awake()
    {
        gameObject.layer = 10;
        gameObject.tag = "Entity";
    }

    public Entity GetParentEntityComponent()
    {
        return parent;
    }
}
