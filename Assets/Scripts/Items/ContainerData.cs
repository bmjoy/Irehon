using Irehon.CloudAPI;
using Irehon.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PairValue<T, A>
{
    public T FirstValue;
    public A SecondValue;
    public PairValue(T First, A Second)
    {
        FirstValue = First;
        SecondValue = Second;
    }
}

public static class ContainerData
{

    public static Dictionary<int, Container> LoadedContainers = new Dictionary<int, Container>();

    public static async Task UpdateLoadedContainer(int id)
    {
        if (!LoadedContainers.ContainsKey(id))
            return;
        
        var www = Api.Request($"/container/{id}", ApiMethod.PUT, LoadedContainers[id].ToJson().ToString());
        await www.SendWebRequest();
    }
    public static async Task<int> CreateCloudContainer(int quantity = 1)
    {
        var www = Api.Request($"/container?quantity={quantity}", ApiMethod.POST);
        await www.SendWebRequest();
        return Api.GetResult(www)["id"].AsInt;
    }


    public static async Task LoadContainer(int containerId)
    {
        if (LoadedContainers.ContainsKey(containerId))
            return;

        var www = Api.Request($"/container/{containerId}");
        await www.SendWebRequest();
        var result = Api.GetResult(www);

        if (result != null)
        {
            LoadedContainers[containerId] = new Container(result);
        }
    }

    public static async Task LoadContainerAsync(int containerId)
    {
        if (LoadedContainers.ContainsKey(containerId))
        {
            return;
        }

        var www = Api.Request($"/container/{containerId}");
        await www.SendWebRequest();
        var result = Api.GetResult(www);

        if (result != null)
            LoadedContainers[containerId] = new Container(result);
    }

    public static void UnLoadContainer(int containerId)
    {
        if (!LoadedContainers.ContainsKey(containerId))
        {
            return;
        }
        LoadedContainers.Remove(containerId);
    }
}
