using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Bow : NetworkBehaviour
{
    public enum ArrowType { Snipe, Common, Recoil };
    private const float HOLD_DELAY = 1f;
    private const float MIN_HOLDING_TIME = 1f;
    private const float MAX_HOLDING_TIME = 2f;
    private const float BASE_ARROW_IMPULSE = 20;
    private const float HOLDING_ARROW_BONUS_IN_SECOND = 45;

    public struct Quiver
    {
        public ArrowType type { get; private set; }
        private Transform quiverTransform;
        private Queue<Arrow> arrowsInQuiever;
        public Quiver(Bow quiverParent, ArrowType arrowType, int arrowQuantityInQuiver, GameObject arrowPrefab)
        {
            arrowsInQuiever = new Queue<Arrow>();
            quiverTransform = new GameObject("quiver").transform;
            quiverTransform.parent = quiverParent.transform;
            type = arrowType;
            for (int i = 0; i < arrowQuantityInQuiver; i++)
            {
                GameObject arrowObj = Instantiate(arrowPrefab, quiverTransform);
                Arrow arrow = arrowObj.GetComponent<Arrow>();
                arrow.SetParent(quiverParent.player, quiverParent.GetPlayerColliderList(), this);
                arrow.gameObject.SetActive(false);
                arrowsInQuiever.Enqueue(arrow);
            }

        }

        public void ReturnArrowInQuiver(Arrow arrow)
        {
            if (!arrowsInQuiever.Contains(arrow))
                arrowsInQuiever.Enqueue(arrow);
            arrow.transform.parent = quiverTransform;
            arrow.gameObject.SetActive(false);
        }

        public Arrow GetArrowFromQuiver()
        {
            if (arrowsInQuiever.Count > 0)
            {
                Arrow arrow = arrowsInQuiever.Dequeue();
                arrow.transform.SetParent(null);
                arrow.gameObject.SetActive(true);
                arrow.ResetArrow();
                return arrow;
            }
            return null;
        }
    };

    [SerializeField]
    private Transform bowTransform;
    [SerializeField]
    private Transform stringBone;
    [SerializeField]
    private Transform rightHand;
    [SerializeField]
    private GameObject snipeProjectile;
    [SerializeField]
    private GameObject commonProjectile;
    [SerializeField]
    private GameObject recoilProjectile;
    [SerializeField]
    private GameObject projectileInHand;
    private SniperArrowParticle sniperArrowParticle;
    private float holdingTime;
    private float previousPull;
    private ArrowType currentArrowType;
    public int x { get; set; }
    public Player player { get; private set; }
    private List<Quiver> avaliableQuivers = new List<Quiver>();
    private AudioSource audioSource;

    private bool aiming;

    private Vector3 stringStartPosition;

    private void Start()
    {
        stringStartPosition = stringBone.localPosition;
        player = GetComponent<Player>();
        sniperArrowParticle = projectileInHand.GetComponent<SniperArrowParticle>();
        avaliableQuivers.Add(new Quiver(this, ArrowType.Snipe, 5, snipeProjectile));
        avaliableQuivers.Add(new Quiver(this, ArrowType.Common, 10, commonProjectile));
        avaliableQuivers.Add(new Quiver(this, ArrowType.Recoil, 2, recoilProjectile));
    }

    private void Update()
    {
        if (aiming)
        {
            holdingTime += Time.deltaTime;
            if (holdingTime > MIN_HOLDING_TIME)
            {
                float pull = (holdingTime - MIN_HOLDING_TIME) / (MAX_HOLDING_TIME - MIN_HOLDING_TIME);
                if (pull > 1)
                    pull = 1;
                if (isLocalPlayer && pull != previousPull)
                {
                    UIController.instance.ChangeTriangleAimSize(pull);
                    sniperArrowParticle.SetWaveSize(pull);
                }
                previousPull = pull;
            }
            if (holdingTime > HOLD_DELAY)
                stringBone.position = rightHand.position;
        }
        else if (stringBone.position != stringStartPosition)
        {
            stringBone.localPosition = Vector3.Lerp(stringBone.localPosition, stringStartPosition, 0.1f);
        }
    }

    public void SetCurrentArrowType(ArrowType currentType)
    {
        currentArrowType = currentType;
    }

    [Command]
    private void SpawnProjectileOnServer(Vector3 target)
    {
        if (currentArrowType == ArrowType.Snipe && holdingTime < MIN_HOLDING_TIME)
            return;
        if (!isLocalPlayer)
        {
            float power = PullAndShootArrow(target, currentArrowType);
            if (power != 0)
                PullAndShootArrow(target, projectileInHand.transform.position, currentArrowType, power);
        }
    }

    public void StartAim()
    {
        aiming = true;
        holdingTime = 0;
        projectileInHand.SetActive(true);
        sniperArrowParticle.SetWaveSize(0);
        previousPull = 0;
        if (!isLocalPlayer) //host
            StartAimOtherPlayers();
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
        if (currentArrowType == ArrowType.Snipe && holdingTime < MIN_HOLDING_TIME)
            return false;
        if (isLocalPlayer)
        {
            if (PullAndShootArrow(target, currentArrowType) == 0)
                return false;
            SpawnProjectileOnServer(target);
        }
        return true;
    }

    private Quiver FindQuiverWithType(ArrowType arrowType)
    {
        foreach (Quiver quiver in avaliableQuivers)
            if (quiver.type == arrowType)
                return quiver;
        return new Quiver();
    }

    private float GetPowerFromType(ArrowType arrowType)
    {
        switch (arrowType)
        {
            case ArrowType.Common:
                return 45;
            case ArrowType.Snipe:
                if (holdingTime > MAX_HOLDING_TIME)
                    holdingTime = MAX_HOLDING_TIME;
                return HOLDING_ARROW_BONUS_IN_SECOND * holdingTime + BASE_ARROW_IMPULSE;
            case ArrowType.Recoil:
                return 70;
            default:
                return 0;
        }
    }

    private float PullAndShootArrow(Vector3 target, ArrowType arrow)
    {
        Arrow releasedProjectile = FindQuiverWithType(arrow).GetArrowFromQuiver();
        if (!releasedProjectile)
            return 0;
        float power = GetPowerFromType(arrow);
        releasedProjectile.transform.position = projectileInHand.transform.position;
        releasedProjectile.transform.LookAt(target);
        releasedProjectile.SetPower(previousPull);
        releasedProjectile.TriggerReleaseEffect();
        releasedProjectile.GetComponent<Rigidbody>().velocity = releasedProjectile.transform.forward * power;
        if (isLocalPlayer)
            releasedProjectile.GetComponent<Cinemachine.CinemachineImpulseSource>()?.GenerateImpulse(CameraController.instance.transform.forward);
        return power;
    }

    [ClientRpc(excludeOwner = true)]
    private void PullAndShootArrow(Vector3 target, Vector3 releasingPosition, ArrowType arrow, float power)
    {
        Arrow releasedProjectile = FindQuiverWithType(arrow).GetArrowFromQuiver();
        if (!releasedProjectile)
            return;
        releasedProjectile.transform.position = releasingPosition;
        releasedProjectile.transform.LookAt(target);
        releasedProjectile.TriggerReleaseEffect();
        releasedProjectile.GetComponent<Rigidbody>().velocity = releasedProjectile.transform.forward * power;
    }

    public void InterruptAiming()
    {
        aiming = false;
        holdingTime = 0;
        projectileInHand.SetActive(false);
        if (!isLocalPlayer) //host
            InterruptAimingOtherPlayers();
    }

    public List<Collider> GetPlayerColliderList()
    {
        return player.GetHitBoxColliderList();
    }
}
