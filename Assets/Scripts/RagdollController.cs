using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ragdollParts = new List<GameObject>();
    [SerializeField]
    private Collider entityBasicCollider;

    private Animator animator;
    private Rigidbody rigBody;
    private List<Collider> ragdollColliders = new List<Collider>();
    private List<Rigidbody> ragdollRigs = new List<Rigidbody>();

    private void Start()
    {
        foreach (GameObject rag in ragdollParts)
        {
            ragdollColliders.Add(rag.GetComponent<Collider>());
            ragdollRigs.Add(rag.GetComponent<Rigidbody>());
        }
        rigBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void ActivateRagdoll()
    {
        foreach (Collider collider in ragdollColliders)
        {
            collider.isTrigger = false;
        }
        foreach (Rigidbody rig in ragdollRigs)
        {
            rig.isKinematic = false;
        }
        if (rigBody)
            rigBody.isKinematic = true;
        if (animator)
            animator.enabled = false;
        if (entityBasicCollider != null)
            entityBasicCollider.enabled = false;
    }

    public void DisableRagdoll()
    {
        foreach (Collider collider in ragdollColliders)
        {
            collider.isTrigger = true;
        }
        foreach (Rigidbody rig in ragdollRigs)
        {
            rig.isKinematic = true;
        }
        if (rigBody)
            rigBody.isKinematic = false;
        if (animator)
            animator.enabled = true;
        if (entityBasicCollider != null)
            entityBasicCollider.enabled = true;
    }
}
