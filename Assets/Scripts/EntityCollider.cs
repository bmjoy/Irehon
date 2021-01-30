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
