using UnityEngine;

struct FurnaceComponent
{
    public Vector3Int pos;
    public InventoryItem? combustible;
    public InventoryItem? furnaceable;
    public InventoryItem? craftResult;

    public bool firing;
    public Craft.Furnaceable result;

    public float combustionTime;
    public float firingTime;
}
