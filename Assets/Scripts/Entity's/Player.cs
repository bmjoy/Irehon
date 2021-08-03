using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using System.Threading.Tasks;
using System.Collections;

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
    private bool isContainerOpened;
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
        else
            Invoke("Test", 2f);
    }

    private void Test()
    {
        
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

    [TargetRpc]
    public void OpenAnotherContainer(string json)
    {
        Container container = new Container(json);
        InventoryManager.instance.OpenContainer(container);
    }

    [Server]
    public void OpenContainer(int containerId)
    {
        openedContainerId = containerId;
        Task.Run(() =>
            OpenAnotherContainer(MySql.ContainerData.i.GetContainer(containerId).ToJson()));
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
    public void InterractAttempToServer(Vector3 interractPos)
    {
        if (Vector3.Distance(interractPos, transform.position) > 3f)
            return;
        RaycastHit hit;
        if (!Physics.Raycast(interractPos + Vector3.up, Vector3.down * 3, out hit, 3, 1 << 12))
            return;
        hit.collider.GetComponent<IInteractable>().Interact(this);
    }

    [TargetRpc]
    private void CloseChest()
    {
        InventoryManager.instance.CloseContainer();
    }

    [Command]
    public void ChestClosedRpc()
    {
        openedContainerId = 0;
    }

    [Server]
    private void UpdateChestData()
    {
        OpenContainer(openedContainerId);
    }

    [Server]
    public void OpenChest(Chest chest)
    {
        if (openedContainerId != 0 || chest == null ||
                Vector3.Distance(chest.gameObject.transform.position, transform.position) > 4f)
            return;

        OpenContainer(chest.ContainerId);
        openedContainerId = chest.ContainerId;
        chest.OnContainerUpdate.AddListener(UpdateChestData);
        Vector3 chestPos= chest.transform.position;
        StartCoroutine(CheckChestDistance());
        IEnumerator CheckChestDistance()
        {
            while (openedContainerId != 0 && Vector3.Distance(chestPos, transform.position) < 4f)
            {
                yield return null;
            }
            openedContainerId = 0;
            chest.OnContainerUpdate.RemoveListener(UpdateChestData);
            CloseChest();
        }
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
        Task.Run(() =>
        {
            if (firstContainerId == secondContainerId)
            {
                MySql.ContainerData.i.SwapSlot(firstContainerId, firstSlot, secondSlot);
                if (firstContainerId == characterData.containerId)
                {
                    characterData.inventory = MySql.ContainerData.i.GetContainer(characterData.containerId);
                    UpdateCharacterData(connectionToClient, characterData);
                }
                else
                    OpenContainer(firstContainerId);
            }
            else
            {
                MySql.ContainerData.i.MoveSlot(firstContainerId, firstSlot, secondContainerId, secondSlot);
                characterData.inventory = MySql.ContainerData.i.GetContainer(characterData.containerId);
                UpdateCharacterData(connectionToClient, characterData);
                if (firstContainerId != characterData.containerId)
                    OpenContainer(firstContainerId);
                else
                    OpenContainer(secondContainerId);
            }
        });
    }

    public override void TakeDamage(DamageMessage damageMessage)
    {
        base.TakeDamage(damageMessage);
        TakeDamageEvent(connectionToClient, damageMessage.damage);
    }
}
