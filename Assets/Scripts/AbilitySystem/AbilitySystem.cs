using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AbilitySystem : NetworkBehaviour
{
    public GameObject AbilityPoolObject => abilityPoolObject;
    public PlayerBonesLinks playerBonesLinks { get; private set; }
    public PlayerMovement playerMovement { get; private set; }
    public Player player { get; private set; }
    public Animator animator { get; private set; }

    [HideInInspector]
    public KeyCode ListeningKey;

    [SyncVar]
    private bool isAbilityCasting;

    [SerializeField]
    private GameObject abilityPoolObject;

    private AudioSource[] audioSources;
    private AbilityBase currentAbility;


    private void Awake()
    {
        player = GetComponent<Player>();
        playerBonesLinks = GetComponent<PlayerBonesLinks>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSources = abilityPoolObject.GetComponents<AudioSource>();
        isAbilityCasting = false;
    }
    public void SetWeapon(Weapon weapon)
    {
        if (currentAbility != null)
            currentAbility.Interrupt();
        currentAbility = weapon.Setup(this);
        ListeningKey = currentAbility.TriggerKey;
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
        AbilityKeyDownClientRpc(target);
    }
    public void AbilityKeyUp(Vector3 target)
    {
        currentAbility.TriggerKeyUp(target);
        AbilityKeyUpClientRpc(target);
    }

    [ClientRpc]
    private void AbilityKeyDownClientRpc(Vector3 target)
    {
        currentAbility.TriggerKeyDown(target);
    }

    [ClientRpc]
    private void AbilityKeyUpClientRpc(Vector3 target)
    {
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

    [ServerCallback]
    public void AbilityInterrupt()
    {
        currentAbility?.Interrupt();
        AbilityInterruptRpc();
    }

    [ClientRpc]
    private void AbilityInterruptRpc()
    {
        currentAbility?.Interrupt();
    }

    [ServerCallback]
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

    [ServerCallback]
    public void SubEvent()
    {
        currentAbility?.SubEvent();
        SubEventRpc();
    }

    [ClientRpc]
    private void SubEventRpc()
    {
        currentAbility?.SubEvent();
    }

    public void SoundEvent()
    {
        currentAbility?.AbilitySoundEvent();
    }

    public void PlaySoundClip(AudioClip clip)
    {
        if (clip == null)
            return;
        var readyAudioSource = Array.Find(audioSources, source => source.isPlaying == false);
        if (readyAudioSource != null)
        {
            readyAudioSource.clip = clip;
            readyAudioSource.Play();
        }
    }

    public void StopPlayingClip(AudioClip clip)
    {
        Array.Find(audioSources, source => source.clip == clip && source.isPlaying == true)?.Stop();
    }

    public bool IsAbilityCasting() => isAbilityCasting;
}
