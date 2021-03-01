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
    public AoeTargetMark AOETargetMark => aoeTargetMark;
    public Player PlayerComponent => player;
    public PlayerMovement PlayerMovementComponent => playerMovement;

    private PlayerMovement playerMovement;
    private Player player;
    private AoeTargetMark aoeTargetMark;
    private Animator animator;
    private PlayerController playerController;
    private AudioSource audioSource;
    private GameObject abilityPoolObject;

    private List<AbilityBase> abilitysPool;
    private List<IAbility> abilitys = new List<IAbility>();

    private IAbility currentlyCastingSkill;

    private bool canTriggerAbility;
    private int slot;

    private float globalCooldownCountdown;

    public bool IsAbilityCasting() => !canTriggerAbility;

    private void Update()
    {
        globalCooldownCountdown -= Time.deltaTime;
    }

    private void Awake()
    {
        abilityPoolObject = new GameObject("AbilityPool", typeof(AudioSource));
        abilitysPool = new List<AbilityBase>(GetComponents<AbilityBase>());

        player = GetComponent<Player>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = abilityPoolObject.GetComponent<AudioSource>();
        aoeTargetMark = GetComponent<AoeTargetMark>();
        abilityPoolObject.transform.parent = transform;
        canTriggerAbility = true;
    }

    private void Start()
    {
        AddNewAbility(1, 2);
        AddNewAbility(3, 1);
        AddNewAbility(4, 3);
        AddNewAbility(2, 0);
    }

    public void AddNewAbility(int id, int slot)
    {
        IAbility ability = abilitysPool.Find(p => p.Id == id);
        if (ability == null)
        {
            Debug.Log("Ability with " + id + " not found");
            return;
        }
        AbilityUIController.instance.SetAbilityOnSlot(ability, slot);
        if (abilitys.Find(p => p.Id == id) != null)
        {
            Debug.Log("Ability already on " + gameObject.name);
            return;
        }
        abilitys.Add(ability);
        ability.OnAbilityCooldown.AddListener(AbilityTriggered);
    }

    public void AbilityKeyDown(KeyCode key, Vector3 target)
    {
        if (globalCooldownCountdown > 0 || !canTriggerAbility || currentlyCastingSkill != null)
            return;
        foreach (IAbility ability in abilitys)
        {
            if (ability.TriggerKey == key)
            {
                print(key);
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
