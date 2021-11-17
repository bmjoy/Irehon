using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class PersonalChest : Chest
{
    [SerializeField]
    private string chestName;

    [SerializeField]
    private int containerCapacity;

    public override async void Interact(Player player)
    {
        PersonalChestInfo personalChest = player.GetCharacterInfo().personalChests.Find(x => x.ChestName == chestName);
        if (personalChest == null)
        {
            personalChest = new PersonalChestInfo(chestName);

            var www = Api.Request($"/containers/?quantity={containerCapacity}", ApiMethod.POST);
            await www.SendWebRequest();
            
            personalChest.ContainerId = Api.GetResult(www)["id"].AsInt;
            await ContainerData.LoadContainer(personalChest.ContainerId);

            var playerInfo = (PlayerConnectionInfo)player.connectionToClient.authenticationData;
            playerInfo.character.personalChests.Add(personalChest);
            player.connectionToClient.authenticationData = playerInfo;
        }
        player.GetComponent<PlayerContainerController>().OpenChest(this, personalChest.ContainerId);
    }

    public override void StopInterract(Player player)
    {
        base.StopInterract(player);
    }
}
