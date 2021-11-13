using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AbilitySystem : NetworkBehaviour
{
    public GameObject AbilityPoolObject => abilityPoolObject;
    public AudioSource AudioSource => audioSource;
    public Animator AnimatorComponent => animator;
    public Player PlayerComponent => player;
    public PlayerMovement PlayerMovementComponent => playerMovement;
    public PlayerBonesLinks PlayerBonesLinks => boneLinks;


    public KeyCode ListeningKey;

    private PlayerBonesLinks boneLinks;
    private PlayerMovement playerMovement;
    private Player player;
    private Animator animator;
    private AudioSource audioSource;
    private GameObject abilityPoolObject;

    private AbilityBase currentAbility;

    [SerializeField]
    private GameObject weaponPrefab;

    [SyncVar]
    private bool isAbilityCasting;

    public bool IsAbilityCasting() => isAbilityCasting;

    public void SetWeapon(Weapon weapon)
    {
        if (currentAbility != null)
            currentAbility.Interrupt();
        currentAbility = weapon.Setup(this);
        ListeningKey = currentAbility.TriggerKey;
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
    }

    public void SendAbilityKeyStatus(bool isKeyPressed, Vector3 target)
    {
        if (!isAbilityCasting && isKeyPressed)
            AbilityKeyDown(target);
        else if (isAbilityCasting && !isKeyPressed)
            AbilityKeyUp(target);
    }

    public void AbilityKeyDown(Vector3 target)
    {
        currentAbility.TriggerKeyDown(target);
        AbilityKeyDownRPC(target);
    }

    [ClientRpc]
    private void AbilityKeyDownRPC(Vector3 target)
    {
        currentAbility.TriggerKeyDown(target);
    }

    [ClientRpc]
    private void AbilityKeyUpRPC(Vector3 target)
    {
        currentAbility.TriggerKeyUp(target);
    }

    public void AbilityKeyUp(Vector3 target)
    {
        currentAbility.TriggerKeyUp(target);
        AbilityKeyUpRPC(target);
    }

    public void AllowTrigger()
    {
        isAbilityCasting = false;
    }

    public void BlockTrigger()
    {
        isAbilityCasting = true;
    }

    public void AbilityInterrupt()
    {
        if (!isServer)
            return;
        currentAbility?.Interrupt();
        AbilityInterruptRpc();
    }

    [ClientRpc]
    private void AbilityInterruptRpc()
    {
        currentAbility?.Interrupt();
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
        currentAbility.AnimationEvent();
    }
}
