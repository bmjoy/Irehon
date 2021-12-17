using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        string sqlCommand = $"UPDATE containers SET slots = '{LoadedContainers[id].ToJson()}' WHERE id = '{id}';";

        await Api.SqlRequest($"/sql/?request={sqlCommand}").SendWebRequest();
    }

    public static async Task UpdateCloudContainer(Container container, int id)
    {
        string sqlCommand = $"UPDATE containers SET slots = '{container.ToJson()}' WHERE id = '{id}';";

        await Api.SqlRequest($"/sql/?request={sqlCommand}").SendWebRequest();
    }

    public static async Task<int> CreateCloudContainer(int quantity = 1)
    {
        var www = Api.Request($"/containers/?quantity={quantity}", ApiMethod.POST);
        await www.SendWebRequest();
        return Api.GetResult(www)["id"].AsInt;
    }


    public static async Task LoadContainer(int containerId)
    {
        if (LoadedContainers.ContainsKey(containerId))
            return;

        var www = Api.Request($"/containers/{containerId}");
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

        var www = Api.Request($"/containers/{containerId}");
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
