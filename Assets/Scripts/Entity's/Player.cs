using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using System.Threading.Tasks;

public struct CharacterData
{
    public Container inventory;
    public int containerId;
    public int characterId;
}

public class OnCharacterDataUpdate : UnityEvent<CharacterData> {}

public class Player : Entity
{
    public bool isDataAlreadyRecieved { get; private set; } = false;
    private int openedContainerId;
    private PlayerController controller;
    private CharacterData characterData;
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
            InventoryManager.instance.PlayerIntialize(this);
            OnHealthChanged.AddListener(UpdateHealthBar);
            HitConfirmEvent.AddListener(controller.HitConfirmed);
            OnTakeDamageEvent.AddListener(controller.TakeDamageEffect);
        }
    }

    [TargetRpc]
    private void UpdateCharacterData(NetworkConnection con, CharacterData data)
    {
        isDataAlreadyRecieved = true;
        characterData = data;
        OnCharacterDataUpdateEvent.Invoke(characterData);
    }

    [TargetRpc]
    public void SendItemDatabase(string json)
    {
        ItemDatabase.instance.DatabaseLoadJson(json);
    }

    [Server]
    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
        UpdateCharacterData(connectionToClient, characterData);
    }

    public CharacterData GetCharacterData() => characterData;

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

    private int GetContainerId(OpenedContainerType type)
    {
        switch (type)
        {
            case OpenedContainerType.Inventory: return characterData.containerId;
            case OpenedContainerType.OtherContainer: return openedContainerId;
            default: return 0;
        };
    }

    [Command]
    public void MoveItem(OpenedContainerType firstType, int firstSlot, OpenedContainerType secondType, int secondSlot)
    {
        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;
        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;
        if (firstContainerId == secondContainerId)
        {
            Task.Run(() =>
            {
                MySql.ContainerData.i.SwapSlot(firstContainerId, firstSlot, secondSlot);
                if (firstContainerId == characterData.containerId)
                {
                    characterData.inventory = MySql.ContainerData.i.GetContainer(characterData.containerId);
                    UpdateCharacterData(connectionToClient, characterData);
                }
            });
        }
        else
        {
            Task.Run(() =>
            {
                MySql.ContainerData.i.MoveSlot(firstContainerId, firstSlot, secondContainerId, secondSlot);
            });
        }
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        base.TakeDamage(damageMessage);
        TakeDamageEvent(connectionToClient, damageMessage.damage);
    }
}
