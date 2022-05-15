using Leopotam.EcsLite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class QuickInventory : MonoBehaviour
{
    [SerializeField] List<CellInventory> cells;

    public List<CellInventory> Cells => cells;

    public Action<int> onItemClicked;

    EcsFilter filter;
    EcsWorld ecsWorld;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;

        filter = ecsWorld.Filter<InventoryItem>().Inc<ItemQuickInventory>().End();

        foreach (var cell in cells)
        {
            cell.labelCount.text = "";
            cell.onItemClick += Item_Clicked;
            cell.OnItemSeted += Item_Seted;
        }

        GlobalEvents.itemTaked.AddListener(UpdateItems);
        GlobalEvents.itemUsing.AddListener(Item_Using);

        UpdateItems();
    }

    private void Item_Seted(CellInventory cell)
    {
        ecsWorld.GetPool<ItemQuickInventory>().Add(cell.EntityItem.Value);

        UpdateItems();
        CheckEmptyCell();
    }

    private void Item_Clicked(int entity)
    {
        onItemClicked?.Invoke(entity);
    }

    private void Item_Using(int entity)
    {
        UpdateItems();
        CheckEmptyCell();
    }

    public void SetItem(DragItem dragItem, CellInventory cell)
    {
        bool found = false;
        ref var dragComponent = ref ecsWorld.GetPool<InventoryItem>().Get(dragItem.entity);

        foreach (var entity in filter)
        {
            ref var component = ref ecsWorld.GetPool<InventoryItem>().Get(entity);

            if (component.blockID == dragComponent.blockID)
            {
                found = true;

                component.count += dragComponent.count;
                Destroy(dragItem.view);
                ecsWorld.DelEntity(dragItem.entity);

                UpdateItems();
                CheckEmptyCell();
            }
        }

        if (!found)
        {
            cell.SetItem(dragItem);
        }
    }

    private void CheckEmptyCell()
    {
        List<CellInventory> withItems = new();

        foreach (var e in filter)
        {
            var cell = cells.Find(c => c.EntityItem != null && c.EntityItem == e);
            withItems.Add(cell);
        }

        var forCheck = cells.Except(withItems).ToList();
        forCheck.ForEach(c => c.Clear());
    }

    public void UpdateItems()
    {
        int idx = 0;

        foreach (var entity in filter)
        {
            var pool = ecsWorld.GetPool<InventoryItem>();
            ref var component = ref pool.Get(entity);

            cells[idx].Init(entity, ref component);

            idx++;
        }
    }

    
}
