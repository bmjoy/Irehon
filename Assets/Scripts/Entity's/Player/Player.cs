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
    public Container equipment;
    public int equipmentContainerId;
    public int containerId;
    public int characterId;
}

public class OnCharacterDataUpdate : UnityEvent<CharacterData> {}

public class Player : Entity
{
    public bool isDataAlreadyRecieved { get; private set; } = false;
    private PlayerController controller;
    private Equipment equipment = new Equipment();
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
        equipment.Update(data.equipment);
        OnCharacterDataUpdateEvent.Invoke(characterData);
    }

    [ClientRpc]
    private void UpdatePublicData(CharacterData data)
    {
        GetComponent<PlayerContainerController>().UpdateEquipmentModelRpc(data.equipment);
    }

    [Server]
    public void SetCharacterData(CharacterData data)
    {
        characterData = data;
        equipment.Update(data.equipment);
        UpdateCharacterData(connectionToClient, characterData);
        CharacterData sharedData = new CharacterData();
        sharedData.equipment = data.equipment;
        UpdatePublicData(sharedData);
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

    public override void TakeDamage(DamageMessage damageMessage)
    {
        base.TakeDamage(damageMessage);
        TakeDamageEvent(connectionToClient, damageMessage.damage);
    }
}
