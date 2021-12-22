using Irehon.CloudAPI;
using Irehon.Utils;
using UnityEngine;

namespace Irehon.Interactable
{
    public class PersonalChest : Chest
    {
        [SerializeField]
        private string chestName;

        [SerializeField]
        private int containerCapacity;

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
            player.GetComponent<PlayerContainerController>().OpenChest(this, ContainerData.LoadedContainers[personalChest.ContainerId]);
        }

        public override void StopInterract(Player player)
        {
            base.StopInterract(player);
        }
    }
}