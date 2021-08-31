using UnityEngine;
using System.Collections;
using Mirror;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

public class PlayerContainerController : NetworkBehaviour
{
    [SerializeField]
    private GameObject deathPrefab;

    private Player player;
    private PlayerModelManager playerModelManager;
    private Chest chest;

    private Dictionary<ContainerType, int> containers;

    private CharacterInfo characterData => player.GetCharacterData();
    private int openedContainerId;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerModelManager = GetComponent<PlayerModelManager>();

        if (isLocalPlayer)
        {
            if (!player.isDataAlreadyRecieved)
                player.OnCharacterDataUpdateEvent.AddListener(Intialize);
            else
                Intialize(player.GetCharacterData());
        }
    }

    public void Intialize(CharacterInfo info)
    {
        player.OnCharacterDataUpdateEvent.RemoveListener(Intialize);
        containers[ContainerType.Equipment] = info.equipment_id;
        containers[ContainerType.Inventory] = info.inventory_id;
    }

    private ContainerType GetContainerType(int containerId)
    {
        foreach (var keyValuePair in containers)
        {
            if (keyValuePair.Value == containerId)
                return keyValuePair.Key;
        }
        return ContainerType.None;
    }

    [TargetRpc]
    public void GetContainerData(int containerId, Container container)
    {
        ContainerType type = GetContainerType(containerId);

        switch (type)
        {
            case ContainerType.Inventory:

                break;
            case ContainerType.Equipment:
                break;
            case ContainerType.Chest:
                break;
        }
    }

    [Server]
    public void SpawnDeathContainer()
    {
        //GameObject deathBody = Instantiate(deathPrefab);
        //NetworkServer.Spawn(deathBody);
        //deathBody.transform.position = player.GetMoldelPosition();
        //deathBody.transform.rotation = transform.rotation;
        //deathBody.GetComponent<RagdollController>().ChangeRagdollState(true);
        //deathBody.GetComponent<DeathContainer>().SetEquipment(characterData.equipment);
        //CharacterData newData = characterData;
        //newData.equipment.Truncate();
        //newData.inventory.Truncate();
        //player.SendCharacterInfo(newData);
        //int deathContainer = 0;
        //var outer = Task.Factory.StartNew(() =>
        //{
        //    List<int> playerContainers = new List<int>() 
        //    { 
        //        characterData.containerId, 
        //        characterData.equipmentContainerId 
        //    };
        //    deathContainer = Api.ContainerData.MoveAllItemsInNewContainer(playerContainers);
        //});
        //StartCoroutine(WaitTask());
        //IEnumerator WaitTask()
        //{
        //    while (!outer.IsCompleted)
        //        yield return null;
        //    deathBody.GetComponent<Chest>().SetChestId(deathContainer);
        //}
    }

    [TargetRpc]
    public void SendItemDatabase(string json)
    {
        ItemDatabase.DatabaseLoadJson(json);
    }

    [TargetRpc]
    private void CloseChest()
    {
        InventoryManager.i.CloseChest();
    }

    [Command]
    public void OtherContainerClosedRpc()
    {
        chest?.OnContainerUpdate.RemoveListener(UpdateChestData);
        chest = null;
        openedContainerId = 0;
    }

    [Server]
    private void UpdateChestData()
    {
        OpenContainer(openedContainerId);
    }

    [Server]
    public void OpenContainer(int containerId)
    {
        openedContainerId = containerId;
        Task.Run(() =>
            OpenAnotherContainer(Api.ContainerData.GetContainer(containerId).ToJson()));
    }

    private int GetContainerId(ContainerType type)
    {
        switch (type)
        {
            case ContainerType.Inventory: return characterData.inventory_id;
            case ContainerType.Chest: return openedContainerId;
            case ContainerType.Equipment: return characterData.equipment_id;
            default: return 0;
        };
    }

    [Server]
    public void OpenChest(Chest chest)
    {
        if (openedContainerId != 0 || chest == null ||
                Vector3.Distance(chest.gameObject.transform.position, transform.position) > 4f)
            return;

        this.chest = chest;

        OpenContainer(chest.ContainerId);
        openedContainerId = chest.ContainerId;
        chest.OnContainerUpdate.AddListener(UpdateChestData);
        
        Vector3 chestPos = chest.transform.position;

        StartCoroutine(CheckChestDistance());

        IEnumerator CheckChestDistance()
        {
            while (openedContainerId != 0 && Vector3.Distance(chestPos, transform.position) < 4f)
            {
                yield return null;
            }
            openedContainerId = 0;
            chest.OnContainerUpdate.RemoveListener(UpdateChestData);
            chest = null;
            CloseChest();
        }
    }

    //from , to
    [Server]
    private void Equip(int equipmentSlot, int inventorySlot)
    {
        Item equipableItem = ItemDatabase.GetItemById(characterData.inventory[inventorySlot].itemId);

        if (equipableItem.type != ItemType.Armor && equipableItem.type != ItemType.Weapon)
            return;
        if ((EquipmentSlot)equipmentSlot != equipableItem.equipmentSlot)
            return;

        StartCoroutine(ContainerData.MoveSlotData(characterData.inventory_id, inventorySlot, characterData.equipment_id, equipmentSlot));
    }

    private bool IsMoveLegal(ContainerType firstType, ContainerType secondType)
    {
        if (firstType == ContainerType.Equipment && secondType != ContainerType.Inventory)
            return false;
        if (secondType == ContainerType.Equipment && firstType != ContainerType.Inventory)
            return false;
        else
            return true;
    }

    [Command]
    //from , to
    public void MoveItem(ContainerType firstType, int firstSlot, ContainerType secondType, int secondSlot)
    {
        if (!IsMoveLegal(firstType, secondType))
            return;

        int firstContainerId = GetContainerId(firstType);
        if (firstContainerId == 0)
            return;

        int secondContainerId = GetContainerId(secondType);
        if (secondContainerId == 0)
            return;

        if (firstType == ContainerType.Inventory && secondType == ContainerType.Equipment)
        {
            Equip(secondSlot, firstSlot);
            return;
        }

        if (firstType == ContainerType.Equipment && secondType == ContainerType.Inventory)
            StartCoroutine(ContainerData.MoveSlotData(characterData.equipment_id, firstSlot, characterData.inventory_id, secondSlot));
        else if (firstContainerId == secondContainerId)
            StartCoroutine(ContainerData.SwapSlot(firstContainerId, firstSlot, secondSlot));
        else
            StartCoroutine(ContainerData.MoveSlotData(firstContainerId, firstSlot, secondContainerId, secondSlot));
    }
}
