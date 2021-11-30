using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AbilitySystem : NetworkBehaviour
{
    public GameObject AbilityPoolObject => abilityPoolObject;
    public Animator AnimatorComponent => animator;
    public Player PlayerComponent => player;
    public PlayerMovement PlayerMovementComponent => playerMovement;
    public PlayerBonesLinks PlayerBonesLinks => boneLinks;

    [HideInInspector]
    public KeyCode ListeningKey;

    private PlayerBonesLinks boneLinks;
    private PlayerMovement playerMovement;
    private Player player;
    private Animator animator;
    private AudioSource[] audioSources;
    [SerializeField]
    private GameObject abilityPoolObject;

    private AbilityBase currentAbility;

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
        player = GetComponent<Player>();
        boneLinks = GetComponent<PlayerBonesLinks>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSources = abilityPoolObject.GetComponents<AudioSource>();
        isAbilityCasting = false;
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
        currentAbility?.AnimationEvent();
    }

    public void PlaySoundClip(AudioClip clip)
    {
        if (clip == null)
            return;
        var readyAudioSource = Array.Find(audioSources, source => source.isPlaying == false);
        readyAudioSource.clip = clip;
        readyAudioSource.Play();
    }

    public void StopPlayingClip(AudioClip clip)
    {
        Array.Find(audioSources, source => source.clip == clip && source.isPlaying == true).Stop();
    }

    public void SoundEvent()
    {
        currentAbility?.AbilitySoundEvent();
    }
}
