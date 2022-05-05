using UnityEngine;

struct InventoryItem
{
    public byte blockID;
    public GameObject view;
    public ItemType itemType;
    public int count;
}

internal enum ItemType
{
    Block,
    Item,
}
