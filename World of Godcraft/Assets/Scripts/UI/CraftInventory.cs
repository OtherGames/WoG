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

    public Action<int> OnItemClicked;

    EcsFilter filter;
    EcsWorld ecsWorld;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;
        filter = ecsWorld.Filter<InventoryItem>().End();

        foreach (var cell in cells)
        {
            cell.OnItemSeted += CraftItem_Seted;
        }
        cellResult.onItemClick += CraftResult_Clicked;
    }

    private void CraftResult_Clicked(int entity)
    {
        RemoveUsedItem();
        OnItemClicked?.Invoke(entity);
    }

    // TODO
    private void RemoveUsedItem()
    {
        foreach (var c in cells)
        {
            if(c.EntityItem != null)
            {
                Destroy(ecsWorld.GetPool<InventoryItem>().Get(c.EntityItem.Value).view);

                ecsWorld.DelEntity(c.EntityItem.Value);
                c.Clear();
            }
        }
    }

    private void CraftItem_Seted(CellCraftInventory cell)
    {
        var craftable = cells.FindAll(c => c.EntityItem != null);

        var craft = Service<Craft>.Get();

        List<byte?> set = new();
        for (int i = 0; i < cells.Count; i++)
        {
            if(cells[i].EntityItem == null)
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

        if (craftableSet != null)
        {
            var result = craft.sets[craftableSet];

            var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
            var dropedBlock = new GameObject("Droped Block");
            dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
            dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(result);
            dropedBlock.AddComponent<DropedBlock>();
            dropedBlock.transform.localScale /= 3f;
            dropedBlock.layer = 5;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = result;
            component.view = dropedBlock;
            component.count = 1;
            component.itemType = ItemType.Block;

            cellResult.Init(entity, ref component);
        }
    }

    public void CheckCellFotClear(int entity)
    {
        if(cellResult.EntityItem == entity)
        {
            cellResult.Clear();
        }
    }

    public CellCraftInventory GetEnteredCell()
    {
        return cells.Find(c => c.IsPointerEntered);
    }
}
