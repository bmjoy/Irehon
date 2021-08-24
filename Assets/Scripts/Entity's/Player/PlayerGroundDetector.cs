using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGroundDetector : MonoBehaviour
{
    public UnityEvent OnLand { get; private set; }

    public bool isGrounded { get; private set; }

    private void Start()
    {
        OnLand = new UnityEvent();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Walkable"))
            isGrounded = true;
        OnLand.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Walkable"))
            isGrounded = false;
    }
}
