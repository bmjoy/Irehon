using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(LootGenerator))]
public class LootableEntity : Entity
{
    [SerializeField, Tooltip("Will be spawn after entity death, should contain Death container component")]
    private GameObject lootContainerPrefab;

    protected override void Start()
    {
        base.Start();
        OnDeathEvent.AddListener(SpawnDeathContainer);
    }

    protected virtual void SpawnDeathContainer()
    {
        if (!isServer)
            return;

        GameObject deadBody = Instantiate(lootContainerPrefab);
        NetworkServer.Spawn(deadBody);

        deadBody.transform.position = transform.position;

        SpawnDeathContainer(deadBody);
    }

    private void SpawnDeathContainer(GameObject deadBody)
    {
        Container lootContainer = GetComponent<LootGenerator>().GenerateLoot();

        deadBody.GetComponent<Chest>().SetChestContainer(lootContainer);
    }
}
