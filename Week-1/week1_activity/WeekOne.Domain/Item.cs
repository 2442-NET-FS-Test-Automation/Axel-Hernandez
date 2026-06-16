namespace WeekOne.Domain;

public class Item
{
    public string ItemName { get; private set; }
    public string ItemDesc { get; private set; }

    public Item(string itemName, string itemDesc)
    {
        ItemName = itemName;
        ItemDesc = itemDesc;
    }
}
