using Mirror;
using System.Collections;
using UnityEngine;

namespace Irehon.Interactable
{
    public class DeathLootBag : LootBag
    {
        private void Start()
        {
            if (this.isServer)
            {
                this.Container.ContainerSlotsChanged += this.CheckIsContainerEmpty;
                this.StartCoroutine(this.SelfDestroyOnTime(240f));
            }
        }

        public void CheckIsContainerEmpty(Container container)
        {
            if (container.GetFilledSlotsCount() == 0)
            {
                this.StartCoroutine(this.SelfDestroyOnTime(4f));
            }
        }

        private IEnumerator SelfDestroyOnTime(float time)
        {
            yield return new WaitForSeconds(time);
            NetworkServer.Destroy(this.gameObject);
        }

        protected override void OnContainerSet()
        {
            base.OnContainerSet();
            this.CheckIsContainerEmpty(this.Container);
        }
    }
}