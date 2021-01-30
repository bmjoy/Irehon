using UnityEngine;
using Mirror;

public class AnimatorController : NetworkBehaviour
{
    protected Animator animator;
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void LateUpdate()
    {
        
    }
}
