using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Abilitys
{
    public class Quiver : MonoBehaviour
    {
        public Transform quiverTransform;
        private Queue<Arrow> arrowsInQuiever;
        public Quiver(Transform parent, Player quiverOwner, int arrowQuantityInQuiver, GameObject arrowPrefab, int damage)
        {
            this.arrowsInQuiever = new Queue<Arrow>();
            this.quiverTransform = new GameObject("quiver").transform;
            this.quiverTransform.parent = parent;
            for (int i = 0; i < arrowQuantityInQuiver; i++)
            {
                GameObject arrowObj = Instantiate(arrowPrefab, this.quiverTransform);
                Arrow arrow = arrowObj.GetComponent<Arrow>();
                arrow.SetParent(quiverOwner, quiverOwner.HitboxColliders, this);
                arrow.SetDamage(damage);
                arrow.gameObject.SetActive(false);
                this.arrowsInQuiever.Enqueue(arrow);
            }
        }

        public void ReturnArrowInQuiver(Arrow arrow)
        {
            if (!this.arrowsInQuiever.Contains(arrow))
            {
                this.arrowsInQuiever.Enqueue(arrow);
            }

            arrow.transform.parent = this.quiverTransform;
            arrow.transform.localPosition = Vector3.zero;
            arrow.transform.localRotation = new Quaternion();
            arrow.gameObject.SetActive(false);
        }

        public Arrow GetArrowFromQuiver()
        {
            print(this.arrowsInQuiever.Count);
            if (this.arrowsInQuiever.Count > 0)
            {
                Arrow arrow = this.arrowsInQuiever.Dequeue();
                arrow.transform.SetParent(null);
                arrow.gameObject.SetActive(true);
                arrow.ResetArrow();
                return arrow;
            }
            return null;
        }

        private void OnDestroy()
        {
            Destroy(this.quiverTransform);
        }
    };
}