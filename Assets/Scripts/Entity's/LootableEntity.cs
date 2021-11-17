using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(LootGenerator))]
public class LootableEntity : Entity
{
    [SerializeField, Tooltip("Will be spawn after entity death, should contain Death container component")]
    private GameObject lootContainerPrefab;

    [SerializeField, Tooltip("Will be hiden after death")]
    private GameObject model;

    protected override void Start()
    {
        base.Start();
        OnDeathEvent.AddListener(SpawnDeathContainer);
        OnDeathEvent.AddListener(() => model.SetActive(false));
    }

    protected virtual void SpawnDeathContainer()
    {
        if (!isServer)
            return;

        GameObject deadBody = Instantiate(lootContainerPrefab);
        NetworkServer.Spawn(deadBody);

        deadBody.transform.position = transform.position;
        deadBody.transform.rotation = transform.rotation;

        SpawnDeathContainer(deadBody);
    }

    protected override void SetDefaultState()
    {
        base.SetDefaultState();
        model.SetActive(true);
    }

    private async void SpawnDeathContainer(GameObject deadBody)
    {
        var www = Api.Request("/containers/?quantity=1", ApiMethod.POST);
        await www.SendWebRequest();
        int newContainerId = Api.GetResult(www)["id"].AsInt;

        Container lootContainer = GetComponent<LootGenerator>().GenerateLoot();

        ContainerData.SaveContainer(newContainerId, lootContainer);

        deadBody.GetComponent<Chest>().SetChestId(newContainerId);
    }
}
