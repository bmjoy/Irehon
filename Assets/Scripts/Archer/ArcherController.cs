using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherController : PlayerController
{
    private bool isAiming;
    private bool aimingMovement;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (previousInput.SprintKeyDown && !abilitySystem.IsAbilityCasting())
            return;
        base.Update();
    }
}
