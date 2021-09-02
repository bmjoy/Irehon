using Mirror;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using System.Threading.Tasks;
using System.Collections;

public class OnCharacterDataUpdate : UnityEvent<CharacterInfo> {}

public class Player : Entity
{
    public bool isDataAlreadyRecieved { get; private set; } = false;
    public OnCharacterDataUpdate OnCharacterDataUpdateEvent = new OnCharacterDataUpdate();

    private PlayerStateMachine stateMachine;
    private PlayerContainerController containerController;
    private CharacterInfo characterData;


    protected override void Awake()
    {
        base.Awake();
        stateMachine = GetComponent<PlayerStateMachine>();
        containerController = GetComponent<PlayerContainerController>();
    }

    protected override void Start()
    {
        base.Start();

        if (isLocalPlayer)
        {
            InventoryManager.i.PlayerIntialize(this);
            CameraController.i.Intialize(this);
            
            OnHealthChanged.AddListener((oldHealth, newHealth) => UIController.instance.SetHealthBarValue(1f * newHealth / maxHealth));

            OnTakeDamageEvent.AddListener(x => CameraController.CreateShake(5f, .3f));
        }
    }

    [Server]
    private void ServerIntialize(CharacterInfo data)
    {
        ContainerData.ContainerUpdateNotifier.Subscribe(data.inventory_id, containerController.SendContainerData);
        ContainerData.ContainerUpdateNotifier.Subscribe(data.equipment_id, containerController.SendContainerData);
        ContainerData.ContainerUpdateNotifier.Subscribe(data.equipment_id, SendEquipmentInfo);
    }

    [Server]
    public void SendCharacterInfo(CharacterInfo data)
    {
        StartCoroutine(SendInfo());

        if (!isDataAlreadyRecieved)
            ServerIntialize(data);

        isDataAlreadyRecieved = true;

        IEnumerator SendInfo()
        {
            yield return ContainerData.LoadContainer(data.equipment_id);
            Container equipment = ContainerData.LoadedContainers[data.equipment_id];

            yield return ContainerData.LoadContainer(data.inventory_id);
            Container inventory = ContainerData.LoadedContainers[data.inventory_id];

            characterData = data;
            UpdateCharacterData(characterData);

            containerController.SendContainerData(data.inventory_id, inventory);
            containerController.SendContainerData(data.equipment_id, equipment);

            SendEquipmentInfo(data.equipment_id, equipment);
        }
    }

    [TargetRpc]
    private void UpdateCharacterData(CharacterInfo data)
    {
        isDataAlreadyRecieved = true;
        characterData = data;
        OnCharacterDataUpdateEvent.Invoke(characterData);
    }

    [ClientRpc]
    private void SendEquipmentInfo(int id, Container equipment)
    {
        GetComponent<PlayerModelManager>().UpdateEquipmentContainer(equipment);
    }

    public CharacterInfo GetCharacterData() => characterData;

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

    private void OnDestroy()
    {
        Debug.Log("Invoked");
        if (isServer)
        {
            ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.inventory_id, containerController.SendContainerData);
            ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.equipment_id, containerController.SendContainerData);
            ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.equipment_id, SendEquipmentInfo);
        }
    }
}
