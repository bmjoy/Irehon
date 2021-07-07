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
    public PlayerController PlayerControll => playerController;
    public Player PlayerComponent => player;
    public PlayerMovement PlayerMovementComponent => playerMovement;

    private PlayerMovement playerMovement;
    private Player player;
    private Animator animator;
    private PlayerController playerController;
    private AudioSource audioSource;
    private GameObject abilityPoolObject;

    private AbilityBase currentAbility;

    private bool isAbilityCasting;
    private bool isAbilityOnCooldown;
    private int slot;

    private float abilityCooldown;

    public bool IsAbilityCasting() => isAbilityCasting;

    private void Update()
    {
        abilityCooldown -= Time.deltaTime;
    }

    private void Awake()
    {
        abilityPoolObject = new GameObject("AbilityPool", typeof(AudioSource));

        player = GetComponent<Player>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = abilityPoolObject.GetComponent<AudioSource>();
        abilityPoolObject.transform.parent = transform;
        isAbilityCasting = false;
    }

    private void Start()
    {
        if (!isLocalPlayer && !isServer)
            return;
    }

    public void AbilityKeyDown(KeyCode key, Vector3 target)
    {
        if (abilityCooldown > 0 || isAbilityCasting)
            return;
        if (currentAbility.TriggerKey == key)
            currentAbility.TriggerKeyDown(target);
    }

    public void AbilityKeyUp(KeyCode key, Vector3 target)
    {
        if (currentAbility.TriggerKey == key)
            currentAbility.TriggerKeyUp(target);
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
    }
}
