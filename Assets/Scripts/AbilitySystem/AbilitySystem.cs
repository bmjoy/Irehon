using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AbilitySystem : NetworkBehaviour, IAbilitySystem
{
    public GameObject AbilityPoolObject => abilityPoolObject;
    public AudioSource AudioSource => audioSource;
    public Animator AnimatorComponent => animator;
    public Player PlayerComponent => player;
    public PlayerMovement PlayerMovementComponent => playerMovement;
    public PlayerBonesLinks PlayerBonesLinks => boneLinks;

    private PlayerBonesLinks boneLinks;
    private PlayerMovement playerMovement;
    private Player player;
    private Animator animator;
    private AudioSource audioSource;
    private GameObject abilityPoolObject;

    private AbilityBase currentAbility;

    [SerializeField]
    private GameObject weaponPrefab;

    private GameObject weapon;

    [SyncVar]
    private bool isAbilityCasting;

    private float abilityCooldown;

    public bool IsAbilityCasting() => isAbilityCasting;

    private void Update()
    {
        abilityCooldown -= Time.deltaTime;
    }

    public void SetWeapon(Weapon weapon)
    {
        if (currentAbility != null)
        {
            currentAbility.Interrupt();
        }
        currentAbility = weapon.Setup(this);
    }

    private void Awake()
    {
        abilityPoolObject = new GameObject("AbilityPool", typeof(AudioSource));

        player = GetComponent<Player>();
        boneLinks = GetComponent<PlayerBonesLinks>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSource = abilityPoolObject.GetComponent<AudioSource>();
        abilityPoolObject.transform.parent = transform;
        isAbilityCasting = false;
    }

    private void Start()
    {
        weapon = Instantiate(weaponPrefab, boneLinks.LeftHand);
        currentAbility = weapon.GetComponent<Weapon>().Setup(this);
        if (!isLocalPlayer && !isServer)
            return;
    }

    public void AbilityKeyDown(KeyCode key, Vector3 target)
    {
        if (abilityCooldown > 0 || isAbilityCasting)
            return;
        if (currentAbility.TriggerKey == key)
        {
            currentAbility.TriggerKeyDown(target);
            AbilityKeyDownRPC(target);
        }
    }

    [ClientRpc]
    private void AbilityKeyDownRPC(Vector3 target)
    {
        if (!isServer)
            currentAbility.TriggerKeyDown(target);
    }

    [ClientRpc]
    private void AbilityKeyUpRPC(Vector3 target)
    {
        if (!isServer)
            currentAbility.TriggerKeyUp(target);
    }

    public void AbilityKeyUp(KeyCode key, Vector3 target)
    {
        if (currentAbility.TriggerKey == key)
        {
            currentAbility.TriggerKeyUp(target);
            AbilityKeyUpRPC(target);
        }
    }

    public void AllowTrigger()
    {
        isAbilityCasting = false;
    }

    public void BlockTrigger()
    {
        isAbilityCasting = true;
    }

    [Server]
    public void AnimationEventTrigger()
    {
        currentAbility.AnimationEvent();
        AnimationEventRPC();
    }

    [ClientRpc]
    private void AnimationEventRPC()
    {
        if (!isServer)
            currentAbility.AnimationEvent();
    }
}
