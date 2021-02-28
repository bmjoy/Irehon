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
    public PlayerController CharController => playerController;

    private Animator animator;
    private PlayerController playerController;
    private AudioSource audioSource;
    private GameObject abilityPoolObject;

    [SerializeField]
    private List<AbilityBase> abilitysPool = new List<AbilityBase>();

    private List<IAbility> abilitys = new List<IAbility>();

    private IAbility currentlyCastingSkill;

    private bool canTriggerAbility;

    private float globalCooldownCountdown;

    public bool IsAbilityCasting() => !canTriggerAbility;

    private void Update()
    {
        globalCooldownCountdown -= Time.deltaTime;
    }

    private void Start()
    {
        abilityPoolObject = new GameObject("AbilityPool", typeof(AudioSource));
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = abilityPoolObject.GetComponent<AudioSource>();
        abilityPoolObject.transform.parent = transform;
        canTriggerAbility = true;
        AddNewAbility(typeof(DischargeAbility));
    }

    public void AddNewAbility(params Type[] ability)
    {
        IAbility abilityComponentCheck = ability[0] as IAbility;
        if (!abilitys.Contains(abilityComponentCheck))
        {
            IAbility abilityComponent = gameObject.AddComponent(ability[0]) as IAbility;
            abilitys.Add(abilityComponent);
            abilityComponent.AbilityInit(this);
            abilityComponent.OnAbilityCooldown.AddListener(AbilityTriggered);
            if (isLocalPlayer)
                AbilityUIController.instance.SetAbilityOnSlot(abilitys[abilitys.Count - 1], abilitys.Count - 1);
        }
    }

    public void AbilityKeyDown(KeyCode key, Vector3 target)
    {
        if (globalCooldownCountdown > 0 || !canTriggerAbility || currentlyCastingSkill != null)
            return;
        foreach (IAbility ability in abilitys)
        {
            if (ability.TriggerKey == key)
            {
                if (ability.TriggerKeyDown(target))
                {
                    SetCurrentSkill(ability);
                }
            }
        }
    }

    [Server]
    public void AbilityTriggered(float time)
    {
        globalCooldownCountdown = 2f;
        InvokeGlobalCooldown(connectionToClient, globalCooldownCountdown);
    }

    [TargetRpc]
    public void InvokeGlobalCooldown(NetworkConnection con, float time)
    {
        AbilityUIController.instance.InvokeGlobalCooldown(time);
    }

    public void AbilityKeyUp(KeyCode key, Vector3 target)
    {
        foreach (IAbility ability in abilitys)
        {
            if (ability.TriggerKey == key && currentlyCastingSkill == ability)
            {
                ability.TriggerKeyUp(target);
            }
        }
    }

    private void SetCurrentSkill(IAbility ability)
    {
        currentlyCastingSkill = ability;
    }

    public void AllowTrigger()
    {
        canTriggerAbility = true;
        SetCurrentSkill(null);
    }

    public void BlockTrigger()
    {
        canTriggerAbility = false;
    }

    [Server]
    public void AnimationEventTrigger()
    {
        if (currentlyCastingSkill != null)
            currentlyCastingSkill.AnimationEvent();
    }
}
