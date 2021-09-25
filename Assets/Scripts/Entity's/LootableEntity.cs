using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LootableEntity : Entity
{
    [SerializeField]
    private GameObject lootContainerPrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            TakeDamage(new DamageMessage { damage = 70, source = this, target = this });
    }

    protected override void Death()
    {
        GameObject deadBody = Instantiate(lootContainerPrefab);
        NetworkServer.Spawn(deadBody);
        
        deadBody.transform.position = transform.position;
        deadBody.transform.rotation = transform.rotation;

        deadBody.GetComponent<Chest>().StartCoroutine(SpawnDeathContainer(deadBody));
        
        base.Death();
    }

    IEnumerator SpawnDeathContainer(GameObject deadBody)
    {

        var www = Api.Request("/containers/?quantity=1", ApiMethod.POST);
        yield return www.SendWebRequest();
        int newContainerId = Api.GetResult(www)["id"].AsInt;

        Container lootContainer = GetComponent<LootGenerator>().GenerateLoot();

        ContainerData.SaveContainer(newContainerId, lootContainer);

        deadBody.GetComponent<Chest>().SetChestId(newContainerId);
    }
}
