using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    private bool isAiming;
    private bool aimingMovement;
    private new ArcherAnimatorController animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<ArcherAnimatorController>();
    }

    protected override void Update()
    {
        if (previousInput.SprintKeyDown)
            return;
        base.Update();
        CheckAbilityTriggerKey(KeyCode.F);
        if (!isLocalPlayer)
            return;
        if (!isControllAllow)
            return;
    }
}
