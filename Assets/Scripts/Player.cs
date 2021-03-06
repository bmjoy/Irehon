using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public struct PlayerInfo : NetworkMessage
{
    public Skill[] skills;
    public int[] unlockedSkills;
}

public class Player : Entity
{
    PlayerController controller;
    

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<PlayerController>();
    }

    protected override void Start()
    {
        PlayerInfo testInfo = new PlayerInfo()
        {
            skills = new Skill[] { new Skill(SkillType.Bow, 5, 12), new Skill(SkillType.Bow, 8, 454) },

            unlockedSkills = new int[] { 1, 4152 }
        };
        string test = JsonUtility.ToJson(testInfo);
        PlayerInfo newInfo = JsonUtility.FromJson<PlayerInfo>(test);
        base.Start();
        if (isLocalPlayer)
        {
            OnHealthChanged.AddListener(UpdateHealthBar);
            HitConfirmEvent.AddListener(controller.HitConfirmed);
            OnTakeDamageEvent.AddListener(controller.TakeDamageEffect);
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
