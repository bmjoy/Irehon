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
    [SerializeField]
    private GameObject deathContainerPrefab;

    public PlayerBonesLinks PlayerBonesLinks { get; private set; }
    public bool isDataAlreadyRecieved { get; private set; } = false;
    public OnCharacterDataUpdate OnCharacterDataUpdateEvent = new OnCharacterDataUpdate();
    public PlayerContainerController ContainerController => containerController;
    public OnContainerUpdate OnPublicEquipmentUpdate { get; private set; } = new OnContainerUpdate();
    private PlayerStateMachine stateMachine;
    private PlayerContainerController containerController;
    private CharacterInfo characterData;


    protected override void Awake()
    {
        base.Awake();
        stateMachine = GetComponent<PlayerStateMachine>();
        containerController = GetComponent<PlayerContainerController>();
        PlayerBonesLinks = GetComponent<PlayerBonesLinks>();
    }

    protected override void Start()
    {
        base.Start();
        
        if (isLocalPlayer)
        {
            HitConfirmEvent.AddListener(x => UIController.instance.ShowHitMarker());

            ContainerWindowManager.i.PlayerIntialize(this);
            CameraController.i.Intialize(this);
            CraftWindowManager.Intialize(this);
            
            OnHealthChanged.AddListener((oldHealth, newHealth) => UIController.instance.SetHealthBarValue(1f * newHealth / maxHealth));

            OnTakeDamageEvent.AddListener(x => CameraController.CreateShake(5f, .3f));
        }

        OnPublicEquipmentUpdate.AddListener(x => print($"Got equip info {x.ToJson()}"));

        if (!isServer)
            OnPublicEquipmentUpdate.AddListener(GetComponent<PlayerModelManager>().UpdateEquipmentContainer);
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


    [Server]
    private void SendEquipmentInfo(int id, Container equip) 
    {
        GetComponent<PlayerWeaponEquipment>().UpdateWeapon(equip);
        SendEquipmentInfoRpc(equip);
    }

    [ClientRpc]
    private void SendEquipmentInfoRpc(Container equipment) => OnPublicEquipmentUpdate.Invoke(equipment);


    public CharacterInfo GetCharacterData() => characterData;

    protected override void SetDefaultState()
    {
        base.SetDefaultState();
        stateMachine.ChangePlayerState(PlayerStateType.Idle);
    }

    protected override void Death() 
    {
        base.Death();


        if (isServer)
        {
            stateMachine.ChangePlayerState(PlayerStateType.Death);
            StartCoroutine(SpawnDeathContainer());
        }

        if (isServerOnly)
            DeathOnClient();
    }

    IEnumerator SpawnDeathContainer()
    {
        GameObject deadBody = Instantiate(deathContainerPrefab);
        NetworkServer.Spawn(deadBody);

        deadBody.transform.position = transform.position;
        deadBody.transform.rotation = transform.rotation;

        yield return ContainerData.LoadContainer(characterData.equipment_id);
        Container equipment = ContainerData.LoadedContainers[characterData.equipment_id];

        deadBody.GetComponent<DeathContainer>().SetEquipment(equipment);

        var www = Api.Request("/containers/?quantity=1", ApiMethod.POST);
        yield return www.SendWebRequest();
        int newContainerId = Api.GetResult(www)["id"].AsInt;

        List<int> characterContainersId = new List<int>();

        characterContainersId.Add(characterData.equipment_id);
        characterContainersId.Add(characterData.inventory_id);
        
        yield return ContainerData.MoveAllItemsInNewContainer(characterContainersId, newContainerId);

        deadBody.GetComponent<Chest>().SetChestId(newContainerId);
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

    public override void TakeDamage(DamageMessage damageMessage)
    {
        base.TakeDamage(damageMessage);
        TakeDamageEvent(damageMessage.damage);
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.inventory_id, containerController.SendContainerData);
            ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.equipment_id, containerController.SendContainerData);
            ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.equipment_id, SendEquipmentInfo);
        }
    }
}
