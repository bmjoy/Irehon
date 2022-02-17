using Irehon;
using Irehon.Abilitys;
using Mirror;
using System;
using UnityEngine;

public class AbilitySystem : NetworkBehaviour
{
    public GameObject AbilityPoolObject => this.abilityPoolObject;
    public PlayerBonesLinks playerBonesLinks { get; private set; }
    public PlayerWeaponEquipment playerWeaponEquipment { get; private set; }
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
        this.player = this.GetComponent<Player>();
        this.playerBonesLinks = this.GetComponent<PlayerBonesLinks>();
        this.playerMovement = this.GetComponent<PlayerMovement>();
        this.animator = this.GetComponent<Animator>();
        playerWeaponEquipment = GetComponent<PlayerWeaponEquipment>();
        this.audioSources = this.abilityPoolObject.GetComponents<AudioSource>();
        this.isAbilityCasting = false;
    }
    public void SetWeapon(Weapon weapon)
    {
        if (this.currentAbility != null)
        {
            this.currentAbility.Interrupt();
        }

        this.currentAbility = weapon.Setup(this);
        this.ListeningKey = this.currentAbility.TriggerKey;
    }

    public void SendAbilityKeyStatus(bool isKeyPressed, Vector3 target)
    {
        if (!this.isAbilityCasting && isKeyPressed)
        {
            this.AbilityKeyDown(target);
        }
        else if (this.isAbilityCasting && !isKeyPressed)
        {
            this.AbilityKeyUp(target);
        }
    }

    public void AbilityKeyDown(Vector3 target)
    {
        this.currentAbility.TriggerKeyDown(target);
        this.AbilityKeyDownClientRpc(target);
    }
    public void AbilityKeyUp(Vector3 target)
    {
        this.currentAbility.TriggerKeyUp(target);
        this.AbilityKeyUpClientRpc(target);
    }

    [ClientRpc]
    private void AbilityKeyDownClientRpc(Vector3 target)
    {
        this.currentAbility?.TriggerKeyDown(target);
    }

    [ClientRpc]
    private void AbilityKeyUpClientRpc(Vector3 target)
    {
        this.currentAbility?.TriggerKeyUp(target);
    }

    public void AllowTrigger()
    {
        this.isAbilityCasting = false;
    }

    public void BlockTrigger()
    {
        this.isAbilityCasting = true;
    }

    [ServerCallback]
    public void AbilityInterrupt()
    {
        this.currentAbility?.Interrupt();
        this.AbilityInterruptRpc();
    }

    [ClientRpc]
    private void AbilityInterruptRpc()
    {
        this.currentAbility?.Interrupt();
    }

    [ServerCallback]
    public void AnimationEventTrigger()
    {
        this.currentAbility.AnimationEvent();
        this.AnimationEventRPC();
    }

    [ClientRpc]
    private void AnimationEventRPC()
    {
        this.currentAbility?.AnimationEvent();
    }

    [ServerCallback]
    public void SubEvent()
    {
        this.currentAbility?.SubEvent();
        this.SubEventRpc();
    }

    [ClientRpc]
    private void SubEventRpc()
    {
        this.currentAbility?.SubEvent();
    }

    public void SoundEvent()
    {
        this.currentAbility?.AbilitySoundEvent();
    }

    public void PlaySoundClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource readyAudioSource = Array.Find(this.audioSources, source => source.isPlaying == false);
        if (readyAudioSource != null)
        {
            readyAudioSource.clip = clip;
            readyAudioSource.Play();
        }
    }

    public void StopPlayingClip(AudioClip clip)
    {
        Array.Find(this.audioSources, source => source.clip == clip && source.isPlaying == true)?.Stop();
    }

    public bool IsAbilityCasting()
    {
        return this.isAbilityCasting;
    }
}
