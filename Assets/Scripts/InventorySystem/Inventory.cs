public class Inventory
{
    public InventorySlot[] slots;

    public Inventory(string json)
    {
        slots = JsonHelper.FromJson<InventorySlot>(json);
    }

    public void FromJsonUpdate(string json) => slots = JsonHelper.FromJson<InventorySlot>(json);

    public string ToJson() => JsonHelper.ToJson(slots);

    public Inventory(InventorySlot[] slots)
    {
        this.slots = slots;
    }

    public Inventory(int capacity)
    {
        slots = new InventorySlot[capacity];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new InventorySlot(i);
    }
}
