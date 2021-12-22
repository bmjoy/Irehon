using Irehon;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EntityBase"))
        {
            other.GetComponent<Player>()?.SelfKill();
        }
    }
}
