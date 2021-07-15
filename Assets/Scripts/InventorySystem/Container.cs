using Utils;

public class Container
{
    public ContainerSlot[] slots;

    public Container(string json)
    {
        slots = JsonHelper.FromJson<ContainerSlot>(json);
    }

    public Container()
    {
    }

    public void FromJsonUpdate(string json) => slots = JsonHelper.FromJson<ContainerSlot>(json);

    public string ToJson() => JsonHelper.ToJson(slots);

    public Container(ContainerSlot[] slots)
    {
        this.slots = slots;
    }

    public Container(int capacity)
    {
        slots = new ContainerSlot[capacity];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new ContainerSlot(i);
    }
}
