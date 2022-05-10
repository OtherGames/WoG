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

    [SerializeField] CraftInventory craftInventory;
    [SerializeField] QuickInventory quickInventory;

    public bool IsShowed { get; set; }
    public float ScreenScale { get; set; }

    List<CellInventory> cells = new();
    DragItem dragItem;
    EcsFilter filterItems;
    EcsFilter filter;
    EcsWorld ecsWorld;

    int size = 18;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;

        filter = ecsWorld.Filter<InventoryItem>().Exc<ItemQuickInventory>().End();
        filterItems = ecsWorld.Filter<InventoryItem>().End();

        for (int i = 0; i < size; i++)
        {
            var cell = Instantiate(cellPrefab, parent);
            cells.Add(cell);
            cell.labelCount.text = "";
        }

        craftInventory.Init();
        craftInventory.OnItemClicked += ItemOnCraft_Clicked;

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

            cells[idx].Init(entity, ref component);

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

    public void ItemClicked(int entity)
    {
        CreateDragItem(entity);
    }

    private void ItemOnCraft_Clicked(int entity)
    {
        CreateDragItem(entity);
    }

    private void CreateDragItem(int entity)
    {
        foreach (var e in filterItems)
        {
            if (e == entity)
            {
                ref var component = ref ecsWorld.GetPool<InventoryItem>().Get(e);
                dragItem = new() { entity = e, view = component.view };
            }
        }
    }

    private void ItemDragStop()
    {
        print("Item Droped");
        var cell = craftInventory.GetEnteredCell();

        if (cell)
        {
            cell.SetItem(dragItem);
            dragItem = null;
        }
        else
        {
            var c = quickInventory.GetEnteredCell();
            if (c)
            {
                c.SetItem(dragItem);
                dragItem = null;
            }
        }
        
    }

    private void ClearStartDragCell()
    {
        var cell = quickInventory.Cells.Find(c => c.EntityItem == dragItem.entity);
        cell?.Clear();

        craftInventory.CheckCellFotClear(dragItem.entity);
    }
    
    private void Update()
    {
        if(dragItem != null)
        {
            float k = transform.lossyScale.x / ScreenScale;
            var t = dragItem.view.transform.parent;
            float x = (Input.mousePosition.x * k) - ((Screen.width / 2) * k);
            float y = (Input.mousePosition.y * k) - ((Screen.height / 2) * k);
            float z = t.position.z;
            t.position = new Vector3(x, y, z);


            if (Input.GetMouseButtonUp(0))
            {
                ClearStartDragCell();
                ItemDragStop();
            }
        }

        

        if (Input.GetKeyDown(KeyCode.V))
        {
            dragItem = null;
        }
    }
}

public class DragItem
{
    public GameObject view;
    public int entity;
}
