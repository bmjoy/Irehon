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

    long frame = 0;

    private void FixedUpdate()
    {
        frame++;
        frameCount.text = $"{frame}";
    }

    public void GiveItem()
    {
        MySql.ContainerData.i.GiveContainerItem(Convert.ToInt32(containerId.text), Convert.ToInt32(itemId.text));
    }

    public void RemoveItem()
    {
        MySql.ContainerData.i.RemoveItem(Convert.ToInt32(containerId.text), Convert.ToInt32(objectId.text));
    }

    public void GiveCharacterItem()
    {
        MySql.ContainerData.i.GiveCharacterItem(Convert.ToInt32(characterId.text), Convert.ToInt32(itemId.text));
    }

    public void ChangeItemOwner()
    {
        var outer = Task.Factory.StartNew(() =>
        {
            MySql.ContainerData.i.ChangeItemOwner(Convert.ToInt32(containerId.text), Convert.ToInt32(container2Id.text), Convert.ToInt32(objectId.text));
        });
    }

    public void CreateContainer()
    {
        MySql.ContainerData.i.CreateContainer(Convert.ToInt32(containerId.text));
    }
}
