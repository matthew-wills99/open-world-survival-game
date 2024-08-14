[System.Serializable]
public class ItemDrop
{
    public Item item;
    public int quantity;
    public float dropChance;

    public ItemDrop(Item item, int quantity, float dropChance)
    {
        this.item = item;
        this.quantity = quantity;
        this.dropChance = dropChance;
    }
}