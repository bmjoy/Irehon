using Irehon.CloudAPI;
using Irehon.Utils;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Irehon.Interactable
{
    public class PersonalChest : Chest
    {
        [SerializeField]
        private string chestName;

        [SerializeField]
        private int containerCapacity;

        private Dictionary<Container, Player> containerBinds = new Dictionary<Container, Player>();

        public override async void Interact(Player player)
        {
            PersonalChestInfo personalChest = player.GetCharacterInfo().personalChests.Find(x => x.ChestName == this.chestName);
            if (personalChest == null)
            {
                personalChest = new PersonalChestInfo(this.chestName);

                var www = Api.Request($"/containers/?quantity={this.containerCapacity}", ApiMethod.POST);
                await www.SendWebRequest();

                personalChest.ContainerId = Api.GetResult(www)["id"].AsInt;
                await ContainerData.LoadContainer(personalChest.ContainerId);

                PlayerSession playerInfo = (PlayerSession)player.connectionToClient.authenticationData;
                playerInfo.character.personalChests.Add(personalChest);
                player.connectionToClient.authenticationData = playerInfo;
            }

            Container personalContainer = ContainerData.LoadedContainers[personalChest.ContainerId];

            TargetOpenChest(player.connectionToClient, personalContainer);
            
            player.GetComponent<PlayerContainers>().interactContainer = personalContainer;
            
            personalContainer.ContainerSlotsChanged += SendPersonalChest;

            containerBinds.Add(personalContainer, player);
        }

        private void SendPersonalChest(Container container)
        {
            if (!containerBinds.ContainsKey(container))
                return;
            Player player = containerBinds[container];

            TargetOpenChest(player.connectionToClient, container);
        }

        public override void StopInterract(Player player)
        {
            PersonalChestInfo personalChest = player.GetCharacterInfo().personalChests.Find(x => x.ChestName == this.chestName);

            Container personalContainer = ContainerData.LoadedContainers[personalChest.ContainerId];

            ContainerData.LoadedContainers[personalChest.ContainerId].ContainerSlotsChanged -= SendPersonalChest;
            containerBinds.Remove(personalContainer);

            base.StopInterract(player);
        }
    }
}