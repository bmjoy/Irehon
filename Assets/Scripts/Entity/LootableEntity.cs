using Irehon.Entitys;
using Irehon.Interactable;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(LootGenerator))]
public class LootableEntity : Entity
{
    [SerializeField, Tooltip("Will be spawn after entity death, should contain Death container component")]
    private GameObject lootContainerPrefab;

    protected override void Start()
    {
        base.Start();
        Dead += this.SpawnDeathContainer;
    }

    protected virtual void SpawnDeathContainer()
    {
        if (!this.isServer)
        {
            return;
        }

        GameObject deadBody = Instantiate(this.lootContainerPrefab);
        NetworkServer.Spawn(deadBody);

        deadBody.transform.position = this.transform.position;

        this.SpawnDeathContainer(deadBody);
    }

    private void SpawnDeathContainer(GameObject deadBody)
    {
        Container lootContainer = this.GetComponent<LootGenerator>().GenerateLoot();

        deadBody.GetComponent<Chest>().SetChestContainer(lootContainer);
    }
}
