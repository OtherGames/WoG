using Leopotam.EcsLite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickInventory : MonoBehaviour
{
    [SerializeField] List<CellQuickInventory> cells;

    EcsFilter filter;
    EcsWorld ecsWorld;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;

        filter = ecsWorld.Filter<InventoryItem>().Inc<ItemQuickInventory>().End();

        foreach (var item in cells)
        {
            item.labelCount.text = "";
        }

        GlobalEvents.itemTaked.AddListener(UpdateItems);

        UpdateItems();
    }

    public void UpdateItems()
    {
        int idx = 0;

        foreach (var entity in filter)
        {
            var pool = ecsWorld.GetPool<InventoryItem>();
            ref var component = ref pool.Get(entity);

            cells[idx].Init(ref component);

            idx++;
        }
    }
}
