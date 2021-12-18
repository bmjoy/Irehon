using UnityEngine;

namespace Irehon.Entitys
{
    public class EntityCollider : MonoBehaviour
    {
        public float damageMultiplier = 1;

        [SerializeField]
        private Entity parent;

        private void Awake()
        {
            this.gameObject.layer = 10;
            this.gameObject.tag = "Entity";
        }

        public Entity GetParentEntityComponent()
        {
            return this.parent;
        }
    }
}