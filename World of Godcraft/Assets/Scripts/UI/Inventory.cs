using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Leopotam.EcsLite;

public class Inventory : MonoBehaviour
{
    [SerializeField] CellInventory cellPrefab;
    [SerializeField] Transform parent;

    public bool IsShowed { get; set; }

    List<CellInventory> cells = new();
    EcsFilter filter;
    EcsWorld ecsWorld;

    int size = 18;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;

        filter = ecsWorld.Filter<InventoryItem>().Exc<ItemQuickInventory>().End();

        for (int i = 0; i < size; i++)
        {
            var cell = Instantiate(cellPrefab, parent);
            cells.Add(cell);
            cell.labelCount.text = "";
        }

        GlobalEvents.itemTaked.AddListener(Item_Taked);
    }

    private void Item_Taked()
    {
        UpdateInventory();
    }

    private void UpdateInventory()
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

    public void Show()
    {
        IsShowed = true;

        gameObject.SetActive(true);

        UpdateInventory();
    }

    public void Hide()
    {
        IsShowed = false;

        gameObject.SetActive(false);

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            print(filter.GetEntitiesCount());
        }

        
    }
}
