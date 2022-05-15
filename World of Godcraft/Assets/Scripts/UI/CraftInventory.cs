using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.EcsLite;
using LeopotamGroup.Globals;
using System.Linq;
using System;

public class CraftInventory : MonoBehaviour
{
    [SerializeField] List<CellCraftInventory> cells;
    [SerializeField] CellInventory cellResult;

    public List<CellCraftInventory> Cells => cells;
    public CellInventory CellResult => cellResult;

    public Action<int> OnItemClicked;

    protected EcsFilter filter;
    protected EcsWorld ecsWorld;
    protected CraftedItem craftedItem;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;
        filter = ecsWorld.Filter<InventoryItem>().End();

        foreach (var cell in cells)
        {
            cell.OnItemSeted += CraftItem_Seted;
            cell.onItemClick += Item_Clicked;
        }
        cellResult.onItemClick += CraftResult_Clicked;
    }

    protected void Item_Clicked(int entity)
    {
        OnItemClicked?.Invoke(entity);

        ClearCraftedItem();

        cellResult.Clear();
    }

    protected void CraftResult_Clicked(int entity)
    {
        RemoveUsedItem();
        OnItemClicked?.Invoke(entity);

        craftedItem = null;
    }

    // TODO
    protected void RemoveUsedItem()
    {
        foreach (var c in cells)
        {
            if(c.EntityItem != null)
            {
                ref var component = ref ecsWorld.GetPool<InventoryItem>().Get(c.EntityItem.Value);
                component.count--;
                if (component.count == 0)
                {
                    Destroy(component.view);

                    ecsWorld.DelEntity(c.EntityItem.Value);
                    c.Clear();
                }
                else
                {
                    c.UpdateItem(ref component);
                }
            }
        }
    }

    protected void CraftItem_Seted()
    {
        AddCraftItemTag();
        Crafting();
    }

    protected virtual void Crafting()
    {
        var craft = Service<Craft>.Get();

        List<byte?> set = GetCraftSet();

        byte?[] craftableSet = null;
        foreach (var item in craft.sets)
        {
            if (item.Key.Count() == set.Count)
            {
                bool isMatched = true;
                int idx = 0;
                foreach (var id in item.Key)
                {
                    if (id != set[idx])
                    {
                        isMatched = false;
                    }
                    idx++;
                }

                if (isMatched)
                {
                    craftableSet = item.Key;
                }
            }
        }

        ClearCraftedItem();

        if (craftableSet != null)
        {
            var result = craft.sets[craftableSet];
            print("======================");
            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Droped Block");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(result.Item1);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = result.Item1;
            component.view = dropedBlock;
            component.count = result.Item2;
            component.itemType = ItemType.Block;

            cellResult.Init(entity, ref component);

            craftedItem = new() { entity = entity, view = dropedBlock };
        }
    }

    protected List<byte?> GetCraftSet()
    {
        List<byte?> set = new();
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].EntityItem == null)
            {
                set.Add(null);
            }
            else
            {
                foreach (var e in filter)
                {
                    if (e == cells[i].EntityItem)
                    {
                        set.Add(ecsWorld.GetPool<InventoryItem>().Get(e).blockID);
                    }
                }
            }
        }

        return set;
    }

    protected void AddCraftItemTag()
    {
        var filterNoneCraft = ecsWorld.Filter<InventoryItem>().Exc<ItemCraftInventory>().End();
        foreach (var entity in filterNoneCraft)
        {
            var cell = cells.Find(c => c.EntityItem != null && c.EntityItem.Value == entity);
            if (cell)
            {
                ecsWorld.GetPool<ItemCraftInventory>().Add(entity);
                return;
            }
        }
    }

    protected void ClearCraftedItem()
    {
        if (craftedItem != null)
        {
            Destroy(craftedItem.view);
            ecsWorld.DelEntity(craftedItem.entity);
            craftedItem = null;
        }
    }

    public void CheckCellForClear(int entity)
    {
        if(cellResult.EntityItem == entity)
        {
            cellResult.Clear();
        }
    }

    public void CheckCraftableItems()
    {
        if (cellResult.EntityItem == null)
            Crafting();
    }

    public class CraftedItem
    {
        public int entity;
        public GameObject view;
    }
}
