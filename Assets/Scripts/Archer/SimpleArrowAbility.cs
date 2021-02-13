using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SimpleArrowAbility : AbilityBase
{
    [SerializeField]
    private GameObject arrow;
    private Animator animator;
    [SerializeField]
    private Transform rightHand;
    private Player player;
    private ArcherMovement movement;
    private delegate void ListeningTarget(Vector3 target);
    private ListeningTarget currentAction;
    private Vector3 currentTarget;
    private Quiver quiver;

    private bool readyToShoot;
    private Vector3 gettedTarget;
    private bool isGotTarget;


    protected new void Start()
    {
        base.Start();
        player = GetComponent<Player>();
        movement = GetComponent<ArcherMovement>();
        animator = GetComponent<Animator>();
        quiver = new Quiver(player, 15, arrow);
    }

    protected override void Ability(Vector3 target)
    {
        animator.SetTrigger("Shoot");
        animator.SetBool("AimingMovement", true);
        currentTarget = target;
        currentAnimationEvent = ArrowTargetSync;
        movement.IsAiming = true;
        if (!isLocalPlayer)
        {
            isGotTarget = false;
            readyToShoot = false;
        }
    }

    private void ArrowTargetSync()
    {
        currentAction = ShootArrow;
        readyToShoot = true;
        if (isLocalPlayer)
        {
            GetTargetAndShoot(CameraController.instance.GetLookingTargetPosition());
            currentAction(CameraController.instance.GetLookingTargetPosition());
        }
        else
        {
            if (isGotTarget)
                currentAction(gettedTarget);
        }
    }

    [Command]
    private void GetTargetAndShoot(Vector3 target)
    {
        isGotTarget = true;
        gettedTarget = target;
        if (readyToShoot && !isLocalPlayer)
            currentAction(target);
        GetTargetAndShootFromServer(target);
    }

    [ClientRpc(excludeOwner = true)]
    private void GetTargetAndShootFromServer(Vector3 target)
    {
        isGotTarget = true;
        gettedTarget = target;
        if (readyToShoot)
            currentAction(target);
    }

    private void ShootArrow(Vector3 target)
    {
        Arrow releasedArrow = quiver.GetArrowFromQuiver();
        float power = 45;
        releasedArrow.transform.position = rightHand.position;
        releasedArrow.transform.LookAt(target);
        releasedArrow.TriggerReleaseEffect();
        releasedArrow.rigidBody.velocity = releasedArrow.transform.forward * power;
        movement.IsAiming = false;
        animator.SetBool("AimingMovement", false);
        if (isLocalPlayer)
            CameraController.instance.CreateShake(3, .1f);
        AbilityEndEvent();
    }

    protected override void InterruptAbility()
    {
        animator.ResetTrigger("Shoot");
        animator.SetBool("AimingMovement", false);
        movement.IsAiming = false;
        currentAnimationEvent = null;
        AbilityEndEvent();
    }

    protected override void StopHoldingAbility(Vector3 target)
    {
    }
}
