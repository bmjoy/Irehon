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
    private List<int> unlockedSkills;
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

    public void TryUnlockAbility(int id)
    {

    }

    private void Start()
    {
        if (!isLocalPlayer && !isServer)
            return;
        AbilityHotbarController.instance.SetAbilitySystem(this);
        unlockedSkills = new List<int> { 1, 3, 4 };
        AbilityTreeController.instance.SetAbilitys(new List<IAbility>(abilitysPool));
        AbilityTreeController.instance.SetUnlockedSkillsInfo(unlockedSkills);
    }

    public void TrySetAbilityToSlot(int id, int slot)
    {
        if (unlockedSkills.Contains(id))
            SetAbilityToSlot(id, slot);
    }

    public void SetAbilityToSlot(int abilityId, int slot)
    {
        IAbility ability = abilitysPool.Find(p => p.Id == abilityId);
        if (ability == null)
        {
            Debug.Log("Ability with " + abilityId + " not found");
            return;
        }


        //Ability on this slot already have ability
        if (AbilityHotbarController.instance.GetAbilityFromSlot(slot) != null)
        {
            IAbility oldAbility = AbilityHotbarController.instance.GetAbilityFromSlot(slot);
            if (abilitys.Contains(oldAbility))
            {
                abilitys.Remove(oldAbility);
                oldAbility.OnAbilityCooldown.RemoveListener(AbilityTriggered);
            }
            AbilityHotbarController.instance.UnSetAbilityFromSlot(slot);
        }

        //Ability already on hotbar
        if (abilitys.Find(p => p.Id == abilityId) != null)
        {
            if (isLocalPlayer)
                AbilityHotbarController.instance.UnSetAbilityFromSlot(ability);
            if (isLocalPlayer)
                AbilityHotbarController.instance.SetAbilityOnSlot(ability, slot);
        }
        else
        {
            AbilityHotbarController.instance.SetAbilityOnSlot(ability, slot);
            abilitys.Add(ability);
            ability.OnAbilityCooldown.AddListener(AbilityTriggered);
        }
    }

    public IAbility GetAbilityById(int id) => abilitysPool.Find(p => p.Id == id);

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
        AbilityHotbarController.instance.InvokeGlobalCooldown(time);
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
