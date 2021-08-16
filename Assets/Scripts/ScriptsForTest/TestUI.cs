using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    public InputField containerId;
    public InputField container2Id;
    public InputField itemId;
    public InputField objectId;
    public InputField characterId;
    public Text frameCount;
    public InputField count;

    long frame = 0;

    private void FixedUpdate()
    {
        frame++;
        frameCount.text = $"{frame}";
    }

    public void GiveItem()
    {
        MySql.ContainerData.GiveContainerItem(Convert.ToInt32(containerId.text), Convert.ToInt32(itemId.text), Convert.ToInt32(count.text));
    }

    public void RemoveItem()
    {
        MySql.ContainerData.RemoveItem(Convert.ToInt32(containerId.text), Convert.ToInt32(objectId.text));
    }

    public void GiveCharacterItem()
    {
        MySql.ContainerData.GiveCharacterItem(Convert.ToInt32(characterId.text), Convert.ToInt32(itemId.text));
    }

    public void ChangeItemOwner()
    {
        var outer = Task.Factory.StartNew(() =>
        {
            MySql.ContainerData.ChangeItemOwner(Convert.ToInt32(containerId.text), Convert.ToInt32(container2Id.text), Convert.ToInt32(objectId.text));
        });
    }

    public void CreateContainer()
    {
        MySql.ContainerData.CreateContainer(Convert.ToInt32(containerId.text));
    }
}
 