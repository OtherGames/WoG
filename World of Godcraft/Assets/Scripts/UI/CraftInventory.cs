using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.EcsLite;
using LeopotamGroup.Globals;
using System.Linq;

public class CraftInventory : MonoBehaviour
{
    [SerializeField] List<CellCraftInventory> cells;
    [SerializeField] CellCraftInventory cellResult;

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

        byte?[] key = null;
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
                    key = item.Key;
                }
            }
        }

        if (key != null)
        {
            var result = craft.sets[key];

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

            cellResult.Init(ref component);
        }
    }

    public CellCraftInventory GetEnteredCell()
    {
        return cells.Find(c => c.IsPointerEntered);
    }
}
