using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
