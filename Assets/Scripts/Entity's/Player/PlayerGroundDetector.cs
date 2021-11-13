using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGroundDetector : MonoBehaviour
{
    public bool isGrounded { get; private set; }

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            print("Grounded");
            isGrounded = true;
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            print("Not grounded");
            isGrounded = false;
        }
    }
}
