using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bow : NetworkBehaviour
{
    private const int ARROWS_IN_QUIVER_QUANTITY = 10;
    private const float HOLD_DELAY = 1f;
    private const float MIN_HOLDING_TIME = 1f;
    private const float MAX_HOLDING_TIME = 1.5f;
    private const float BASE_ARROW_IMPULSE = 10f;
    private const float HOLDING_ARROW_BONUS_IN_SECOND = 30f;
    [SerializeField]
    private Transform bowTransform;
    [SerializeField]
    private Transform stringBone;
    [SerializeField]
    private Transform rightHand;
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private GameObject projectileInHand;
    private float holdingTime;
    private float lastServerHoldingTime;
    private float previousSize;
    private Player player;
    private PlayerController playerController;
    private GameObject quiver;
    private Queue<Arrow> arrowsInQuiver = new Queue<Arrow>();


    private bool aiming;

    private Vector3 stringStartPosition;

    private void Start()
    {
        stringStartPosition = stringBone.localPosition;
        playerController = GetComponent<PlayerController>();
        player = GetComponent<Player>();
        quiver = new GameObject("Quiver");
        quiver.transform.parent = transform;
        for (int i = 0; i < ARROWS_IN_QUIVER_QUANTITY; i++)
        {
            GameObject arrowObject = Instantiate(projectile);
            arrowObject.transform.parent = quiver.transform;
            Arrow arrow = arrowObject.GetComponent<Arrow>();
            arrow.SetParentBow(this);
            arrow.gameObject.SetActive(false);
            arrow.SetRegisteredColliders(player.GetHitBoxColliderList());
            arrowsInQuiver.Enqueue(arrow);
        }
    }

    private void Update()
    {
        if (aiming)
        {
            holdingTime += Time.deltaTime;
            if (holdingTime > MIN_HOLDING_TIME) 
            {
                float size = (holdingTime - MIN_HOLDING_TIME) / MAX_HOLDING_TIME;
                if (size > 1)
                    size = 1;
                if (isLocalPlayer && size != previousSize)
                    UIController.instance.ChangeTriangleAimSize(size);
                previousSize = size;
            }
            if (holdingTime > HOLD_DELAY)
                stringBone.position = rightHand.position;
        }
        else if (stringBone.position != stringStartPosition)
        {
            stringBone.localPosition = Vector3.Lerp(stringBone.localPosition, stringStartPosition, 0.1f);
        }
    }

    [Command]
    private void SpawnProjectileOnServer(Vector3 target)
    {
        if (isLocalPlayer)
            return;
        holdingTime = lastServerHoldingTime;
        PullAndShootArrow(target);
        SpawnProjectileOnPlayers(target, projectileInHand.transform.position, holdingTime);
    }

    [ClientRpc(excludeOwner = true)]
    private void SpawnProjectileOnPlayers(Vector3 target, Vector3 pos, float power)
    {
        PullAndShootArrow(target, pos, power);
    }

    public void StartAim()
    {
        aiming = true;
        holdingTime = 0;
        projectileInHand.SetActive(true);
        previousSize = 0;
        if (!isLocalPlayer) //host
            StartAimOtherPlayers();
    }

    [Server]
    public void HittedColliderProcess(Collider collider, Arrow arrow)
    {
        if (collider.CompareTag("Entity"))
        {
            collider.GetComponent<EntityCollider>().GetParentEntityComponent().TakeDamageOnServer(arrow.GetDamage());
            playerController.HitConfirmed(connectionToClient);
        }
    }


    [ClientRpc(excludeOwner = true)]
    public void StartAimOtherPlayers()
    {
        aiming = true;
        holdingTime = 0;
        projectileInHand.SetActive(true);
    }

    [ClientRpc(excludeOwner = true)]
    public void InterruptAimingOtherPlayers()
    {
        aiming = false;
        holdingTime = 0;
        projectileInHand.SetActive(false);
    }

    public bool ReleaseProjectile(Vector3 target)
    {
        if (holdingTime < MIN_HOLDING_TIME)
            return false;
        if (playerController.isOwner())
        {
            PullAndShootArrow(target);
            SpawnProjectileOnServer(target);
        }
        else
            lastServerHoldingTime = holdingTime;
        return true;
    }

    private void PullAndShootArrow(Vector3 target)
    {
        Arrow releasedProjectile = GetArrowFromQuiver();
        releasedProjectile.transform.position = projectileInHand.transform.position;
        releasedProjectile.transform.LookAt(target);
        if (holdingTime > MAX_HOLDING_TIME)
            holdingTime = MAX_HOLDING_TIME;
        releasedProjectile.GetComponent<Rigidbody>().velocity = releasedProjectile.transform.forward * (HOLDING_ARROW_BONUS_IN_SECOND * holdingTime + BASE_ARROW_IMPULSE);
        if (isLocalPlayer)
            releasedProjectile.GetComponent<Cinemachine.CinemachineImpulseSource>().GenerateImpulse(CameraController.instance.transform.forward);
    }

    private void PullAndShootArrow(Vector3 target, Vector3 releasingPosition, float power)
    {
        Arrow releasedProjectile = GetArrowFromQuiver();
        releasedProjectile.transform.position = releasingPosition;
        releasedProjectile.transform.LookAt(target);
        releasedProjectile.GetComponent<Rigidbody>().velocity = releasedProjectile.transform.forward * (HOLDING_ARROW_BONUS_IN_SECOND * power + BASE_ARROW_IMPULSE);
    }

    public void InterruptAiming()
    {
        aiming = false;
        holdingTime = 0;
        projectileInHand.SetActive(false);
        if (!isLocalPlayer) //host
            InterruptAimingOtherPlayers();
    }

    public void ReturnArrowInQuiver(Arrow arrow)
    {
        if (!arrowsInQuiver.Contains(arrow))
            arrowsInQuiver.Enqueue(arrow);
        arrow.transform.parent = quiver.transform;
        arrow.gameObject.SetActive(false);
    }

    private Arrow GetArrowFromQuiver()
    {
        if (arrowsInQuiver.Count > 0)
        {
            Arrow arrow = arrowsInQuiver.Dequeue();
            arrow.transform.SetParent(null);
            arrow.gameObject.SetActive(true);
            arrow.ResetArrow();
            return arrow;
        }
        return null;
    }
}
