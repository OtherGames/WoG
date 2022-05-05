using UnityEngine;

struct InventoryItem
{
    public byte blockID;
    public GameObject view;
    public ItemType itemType;
}

internal enum ItemType
{
    Block,
    Item,
}
