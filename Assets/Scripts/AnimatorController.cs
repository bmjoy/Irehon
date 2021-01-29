using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AnimatorController : NetworkBehaviour
{
    protected Animator animator;
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }
}
