using UnityEngine;
using Mirror;

public class EntityAnimatorController : NetworkBehaviour
{
    protected Entity parent;
    protected Animator animator;
    protected virtual void Awake()
    {
        parent = GetComponent<Entity>();
        animator = GetComponent<Animator>();
    }

    public void PlayDeathAnimation()
    {
        animator.SetBool("Dead", true);
    }

    public void RespawnAnimation()
    {
        animator.SetBool("Dead", false);
    }

    protected virtual void LateUpdate()
    {
        
    }
}
