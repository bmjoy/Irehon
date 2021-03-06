using Mirror;
using UnityEngine;

public class SimpleArrowAbility : AbilityBase
{
    public override AbilityUnlockRequirment UnlockRequirment { get; } = new AbilityUnlockRequirment(SkillType.Bow, 0, new int[] { 0 });
    public override int Id { get; } = 3;

    public override string Describe => "Simple bow shot";
    public override string Title => "Quick shot";

    [SerializeField]
    private GameObject arrow;

    private Animator animator;
    private Transform rightHand;
    private Player player;
    private ArcherMovement movement;
    private delegate void ListeningTarget(Vector3 target);
    private ListeningTarget currentAction;
    private Quiver quiver;

    private bool readyToShoot;
    private Vector3 gettedTarget;
    private bool isGotTarget;


    protected override void Start()
    {
        base.Start();
        player =  abilitySystem.PlayerComponent;
        movement = abilitySystem.GetComponent<ArcherMovement>();
        animator = abilitySystem.AnimatorComponent;
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        quiver = new Quiver(abilitySystem.AbilityPoolObject.transform, player, 15, arrow);
    }

    protected override void Ability(Vector3 target)
    {
        animator.SetTrigger("Shoot");
        animator.SetBool("AimingMovement", true);
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
