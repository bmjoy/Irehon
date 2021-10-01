using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerData", menuName = "ScriptableObjects/ServerData", order = 1)]
public class ServerData : ScriptableObject
{
    public List<GameObject> spawnablePrefabs;
}
