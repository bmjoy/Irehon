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
    private PlayerStateMachine stateMachine;
    private Equipment equipment = new Equipment();
    private CharacterData characterData;
    public OnCharacterDataUpdate OnCharacterDataUpdateEvent = new OnCharacterDataUpdate();


    protected override void Awake()
    {
        base.Awake();
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    protected override void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            InventoryManager.i.PlayerIntialize(this);
            OnHealthChanged.AddListener(UpdateHealthBar);
            CameraController.i.Intialize(this);
            OnTakeDamageEvent.AddListener(x => CameraController.CreateShake(5f, .3f));
        }
    }

    private void Update()
    {
        //if (isServer && Input.GetKeyDown(KeyCode.Minus))
        //    TakeDamage(new DamageMessage()
        //    {
        //        damage = 123,
        //        target = this
        //    });
    }

    [TargetRpc]
    private void UpdateCharacterData(NetworkConnection con, CharacterData data)
    {
        isDataAlreadyRecieved = true;
        characterData = data;
        equipment.Update(data.equipment);
        OnCharacterDataUpdateEvent.Invoke(characterData);
    }

    public Vector3 GetMoldelPosition() => Vector3.zero;

    [ClientRpc]
    private void GetPublicCharacterData(CharacterData data)
    {
        CharacterData sharedData = new CharacterData();
        sharedData.equipment = characterData.equipment;
        GetComponent<PlayerModelManager>().UpdateEquipmentContainer(data.equipment);
    }

    [Server]
    public void SetCharacterData(CharacterData data)
    {
        characterData = data;

        equipment.Update(data.equipment);

        UpdateCharacterData(connectionToClient, characterData);

        CharacterData sharedData = new CharacterData();
        sharedData.equipment = data.equipment;

        GetPublicCharacterData(sharedData);
    }

    public CharacterData GetCharacterData() => characterData;

    protected void UpdateHealthBar(int oldHealth, int newHealth)
    {
        UIController.instance.SetHealthBarValue(1f * newHealth / maxHealth);
    }

    protected override void SetDefaultState()
    {
        base.SetDefaultState();
        stateMachine.ChangePlayerState(PlayerStateType.Idle);
    }

    protected override void Death() 
    {
        base.Death();
        if (isServerOnly)
            DeathOnClient();
    }

    protected override void Respawn()
    {
        base.Respawn();
        if (isServer)
            RespawnOnClient();
    }

    [ClientRpc]
    protected virtual void DeathOnClient() => Death();

    [ClientRpc]
    protected virtual void RespawnOnClient() => Respawn();

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
