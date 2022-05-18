using UnityEngine;

struct InventoryItem
{
    public byte blockID;
    public GameObject view;
    public Vector3 rotation;
    public ItemType itemType;
    public int count;
}

public enum ItemType
{
    Block,
    Item,
}
