using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public struct CharacterData
{
    public int lvl;
    public int freeSkillPoints;
}

public class OnCharacterDataUpdate : UnityEvent<CharacterData> {}

public class Player : Entity
{
    private PlayerController controller;
    private CharacterData characterData;
    private Skill[] skills;
    public OnCharacterDataUpdate OnCharacterDataUpdateEvent = new OnCharacterDataUpdate();


    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<PlayerController>();
    }

    protected override void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            OnHealthChanged.AddListener(UpdateHealthBar);
            HitConfirmEvent.AddListener(controller.HitConfirmed);
            OnTakeDamageEvent.AddListener(controller.TakeDamageEffect);
        }
    }

    [TargetRpc]
    private void UpdateCharacterData(NetworkConnection con, CharacterData data)
    {
        characterData = data;
        print(JsonUtility.ToJson(data));
        OnCharacterDataUpdateEvent.Invoke(characterData);
    }

    [TargetRpc]
    private void UpdateSkillData(Skill[] skills)
    {
        this.skills = skills;
        print(skills[0].Name);
    }

    [Server]
    public void SetSkillsData(Skill[] skills)
    {
        this.skills = skills;
    }

    [Server]
    public void SetCharacterData(CharacterData data)
    {
        print(JsonUtility.ToJson(data) + " sending");
        characterData = data;
        UpdateCharacterData(connectionToClient, characterData);
    }

    public Skill[] GetSkills() => skills;

    public CharacterData GetCharacterData() => characterData;

    public virtual void GetSkillXp(SkillType type, int xp)
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].Type == type)
            {
                skills[i].GetXp(xp);
            }
        }
    }

    protected void UpdateHealthBar(int oldHealth, int newHealth)
    {
        UIController.instance.SetHealthBarValue(1f * newHealth / maxHealth);
}

    protected override void SetDefaultState()
    {
        base.SetDefaultState();
        controller.AllowControll();
    }

    protected override void Death()
    {
        base.Death();
        controller.BlockControll();
        if (isServer && !isLocalPlayer)
            DeathOnClient(connectionToClient);
    }

    protected override void Respawn()
    {
        base.Respawn();
        //controller.ResetControll();
        if (isServer && !isLocalPlayer)
            RespawnOnClient(connectionToClient);
    }

    [TargetRpc]
    protected virtual void DeathOnClient(NetworkConnection con)
    {
        base.Death();
        controller.BlockControll();
    }

    [TargetRpc]
    protected virtual void RespawnOnClient(NetworkConnection con)
    {
        base.Respawn();
        //controller.ResetControll();
    }

    [Server]
    public void DoDamage(Entity target, int damage)
    {
        DamageMessage damageMessage = new DamageMessage
        {
            damage = damage,
            source = this,
            target = target
        };
        target.TakeDamage(damageMessage);
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        base.TakeDamage(damageMessage);
        TakeDamageEvent(connectionToClient, damageMessage.damage);
    }
}
