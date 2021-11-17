using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quiver : MonoBehaviour
{
    private Transform quiverTransform;
    private Queue<Arrow> arrowsInQuiever;
    public Quiver(Transform parent, Player quiverOwner, int arrowQuantityInQuiver, GameObject arrowPrefab, int damage)
    {
        arrowsInQuiever = new Queue<Arrow>();
        quiverTransform = new GameObject("quiver").transform;
        quiverTransform.parent = parent;
        for (int i = 0; i < arrowQuantityInQuiver; i++)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, quiverTransform);
            Arrow arrow = arrowObj.GetComponent<Arrow>();
            arrow.SetParent(quiverOwner, quiverOwner.HitboxColliders, this);
            arrow.SetDamage(damage);
            arrow.gameObject.SetActive(false);
            arrowsInQuiever.Enqueue(arrow);
        }
    }

    public void ReturnArrowInQuiver(Arrow arrow)
    {
        if (!arrowsInQuiever.Contains(arrow))
            arrowsInQuiever.Enqueue(arrow);
        arrow.transform.parent = quiverTransform;
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.localRotation = new Quaternion();
        arrow.gameObject.SetActive(false);
    }

    public Arrow GetArrowFromQuiver()
    {
        if (arrowsInQuiever.Count > 0)
        {
            Arrow arrow = arrowsInQuiever.Dequeue();
            arrow.transform.SetParent(null);
            arrow.gameObject.SetActive(true);
            arrow.ResetArrow();
            return arrow;
        }
        return null;
    }

    private void OnDestroy()
    {
        Destroy(quiverTransform);
    }
};
