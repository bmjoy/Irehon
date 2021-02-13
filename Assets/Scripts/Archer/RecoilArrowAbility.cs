using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilArrowAbility : AbilityBase
{
    [SerializeField]
    private ParticleSystem prepareParticle;
    [SerializeField]
    private ParticleSystem releaseParticle;
    [SerializeField]
    private GameObject arrowPrefab;
    [SerializeField]
    private Transform rightHandTransform;

    [SerializeField]
    private float recoilDuration;
    [SerializeField]
    private float recoilPower;

    private Quiver quiver;
    private PlayerController controller;
    private Animator animator;
    private Player player;
    private Vector3 target;
    private Transform releaseParticleOldTransform;
    private Coroutine currentRecoil;

    protected new void Start()
    {
        base.Start();
        releaseParticleOldTransform = releaseParticle.transform.parent;
        player = GetComponent<Player>();
        controller = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        quiver = new Quiver(player, 3, arrowPrefab);
    }

    protected override void Ability(Vector3 target)
    {
        releaseParticle.transform.parent = releaseParticleOldTransform;
        releaseParticle.transform.localPosition = Vector3.zero;
        releaseParticle.transform.localRotation = Quaternion.Euler(Vector3.zero);
        prepareParticle.Play();
        this.target = target;
        controller.BlockControll();
        animator.SetInteger("CurrentSkill", 1);
        animator.SetTrigger("Skill");
        currentAnimationEvent = ShootArrow;
    }

    private void ShootArrow()
    {
        releaseParticle.transform.parent = null;
        releaseParticle.Play();

        Arrow releasedArrow = quiver.GetArrowFromQuiver();
        releasedArrow.transform.position = rightHandTransform.position;
        releasedArrow.transform.LookAt(target);
        releasedArrow.SetPower(1);
        releasedArrow.TriggerReleaseEffect();
        releasedArrow.rigidBody.velocity = releasedArrow.transform.forward * 60;

        currentRecoil = StartCoroutine(recoilFromShoot());

        controller.AllowControll();
        animator.SetInteger("CurrentSkill", 0);
        currentAnimationEvent = null;

        if (isLocalPlayer)
            CameraController.instance.CreateShake(10, .3f);

        AbilityEndEvent();
    }

    private IEnumerator recoilFromShoot()
    {
        float currentDuration = 0;
        Vector3 recoilVector = -transform.forward;
        while (currentDuration < recoilDuration)
        {
            currentDuration += Time.deltaTime;
            transform.position += recoilVector * recoilPower * Time.deltaTime;
            yield return null;
        }
    }

    protected override void InterruptAbility()
    {
        controller.AllowControll();
        animator.SetInteger("CurrentSkill", 0);
        currentAnimationEvent = null;
        if (currentRecoil != null)
        {
            StopCoroutine(currentRecoil);
        }
        AbilityEndEvent();
    }

    protected override void StopHoldingAbility(Vector3 target) {}

    /*
    if (currentRecoilCooldown > 0)
            return;
        recoilStartParticle.Play();
        archerController.ResetState();
        recoilReleaseParticle.Play();
        currentWeapon.SetCurrentArrowType(Bow.ArrowType.Recoil);
        playingSkill = Skill.RecoilShot;
        archerController.Shoot((int) Skill.RecoilShot);
        currentRecoilCooldown = skillRecoilCooldown;
        archerController.BlockControll();
    */
}
