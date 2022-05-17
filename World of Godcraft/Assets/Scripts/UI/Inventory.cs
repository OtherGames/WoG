using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;
using Leopotam.EcsLite;
using UnityEngine.EventSystems;
using LeopotamGroup.Globals;

public class Inventory : MonoBehaviour
{
    [SerializeField] CellInventory cellPrefab;
    [SerializeField] Transform parent;

    [SerializeField] CraftInventory craftInventory;
    [SerializeField] QuickInventory quickInventory;

    public bool IsShowed { get; set; }
    public float ScreenScale { get; set; }

    readonly List<CellInventory> cells = new();
    EcsPool<InventoryItem> poolItems;
    DragItem dragItem;
    EcsFilter filterItems;
    EcsFilter filter;
    EcsWorld ecsWorld;

    int size = 18;

    public void Init()
    {
        ecsWorld = FindObjectOfType<WorldOfGodcraft>().EcsWorld;

        filter = ecsWorld.Filter<InventoryItem>().Exc<ItemQuickInventory>().Exc<ItemCraftInventory>().Exc<StandaloneInventory>().End();
        filterItems = ecsWorld.Filter<InventoryItem>().End();
        poolItems = ecsWorld.GetPool<InventoryItem>();

        for (int i = 0; i < size; i++)
        {
            var cell = Instantiate(cellPrefab, parent);
            cells.Add(cell);
            cell.labelCount.text = "";
            cell.onItemClick += ItemClicked;
        }

        craftInventory.Init();
        craftInventory.OnItemClicked += ItemOnCraft_Clicked;

        GlobalEvents.itemTaked.AddListener(Item_Taked);

        dragItem = null;
    }

    private void Item_Taked()
    {
        UpdateInventory();
    }

    public void UpdateInventory()
    {
        int idx = 0;
        foreach (var entity in filter)
        {
            if (dragItem != null && dragItem.entity == entity)
                continue;

            if (craftInventory.CellResult.EntityItem == entity)
                continue;

            var pool = ecsWorld.GetPool<InventoryItem>();
            ref var component = ref pool.Get(entity);

            cells[idx].Init(entity, ref component);

            idx++;
        }
    }

    public void Show(Vector3Int blockPos)
    {
        quickInventory.onItemClicked += ItemClicked;

        IsShowed = true;

        gameObject.SetActive(true);

        craftInventory.OnShow(blockPos);
        UpdateInventory();
    }

    public void Hide()
    {
        quickInventory.onItemClicked -= ItemClicked;
        craftInventory.OnHide();

        IsShowed = false;

        gameObject.SetActive(false);
    }

    private void ItemClicked(int entity)
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
                dragItem = new() { entity = e, view = component.view, count = component.count };
            }
        }
    }

    private bool ItemDragStop()
    {
        bool result = false;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        var raycasts = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycasts);
        foreach (var raycast in raycasts)
        {
            var cell = raycast.gameObject.GetComponent<CellCraftInventory>();

            if (cell)
            {
                cell.SetItem(dragItem);
                dragItem = null;
                result = true;
            }
            else
            {
                var cellInventory = raycast.gameObject.GetComponent<CellInventory>();

                if (cellInventory)
                {
                    SetItem(cellInventory);
                    dragItem = null;
                    result = true;
                }
            }
        }
        return result;
    }

    private void SetItem(CellInventory cell)
    {
        bool found = false;
        ref var dragComponent = ref ecsWorld.GetPool<InventoryItem>().Get(dragItem.entity);
        
        foreach (var entity in filter)
        {
            if (dragItem.entity == entity)
                continue;

            ref var component = ref ecsWorld.GetPool<InventoryItem>().Get(entity);

            if (component.blockID == dragComponent.blockID)
            {
                found = true;

                component.count += dragComponent.count;
                Destroy(dragItem.view);
                ecsWorld.DelEntity(dragItem.entity);
                
            }
        }

        if (!found)
        {
            cell.SetItem(dragItem);
        }

        UpdateInventory();
        CheckEmptyCell();

        StartCoroutine(Delay());

        // HOT FIX
        IEnumerator Delay()
        {
            yield return null;

            UpdateInventory();
            CheckEmptyCell();
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

    private void ClearCellStartDrag()
    {
        var craftCell = craftInventory.Cells.Find(c => c.EntityItem == dragItem.entity);
        if(craftCell != null)
        {
            ecsWorld.GetPool<ItemCraftInventory>().Del(craftCell.EntityItem.Value);
            craftCell.Clear();
        }

        var cell = quickInventory.Cells.Find(c => c.EntityItem == dragItem.entity);
        if (cell != null)
        {
            ecsWorld.GetPool<ItemQuickInventory>().Del(cell.EntityItem.Value);
            cell.Clear();
        }
        else
        {
            cell = cells.Find(c => c.EntityItem == dragItem.entity);
            if (cell != null)
            {
                cell.Clear();
            }
        }

        craftInventory.CheckCellForClear(dragItem.entity);
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
                ClearCellStartDrag();
                if (ItemDragStop())
                    craftInventory.CheckCraftableItems();
            }

            if (Input.GetMouseButtonDown(1))
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var result = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, result);
                foreach (var element in result)
                {
                    var cell = element.gameObject.GetComponent<CellCraftInventory>();
                    
                    if (cell && cell.EntityItem == null)
                    {
                        ref var component = ref poolItems.Get(dragItem.entity);
                        
                        if (component.count > 1)
                        {
                            component.count--;
                            dragItem.count--;
                            var splitItem = CreateSplitItem(component.blockID);
                            cell.SetItem(splitItem);
                            UpdateDragItem();
                        }
                        else
                        {
                            ClearCellStartDrag();
                            cell.SetItem(dragItem);
                            dragItem = null;
                            craftInventory.CheckCraftableItems();
                        }
                        


                        
                    }
                }

                //ClearStartDragCell();
                //if (ItemDragStop())
                //    craftInventory.CheckCraftableItems();
            }
        }

        

        if (Input.GetKeyDown(KeyCode.V))
        {
            print(dragItem.view);
            dragItem = null;
        }
    }

    private DragItem CreateSplitItem(byte blockID)
    {
        var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
        var dropedBlock = new GameObject("Droped Block");
        dropedBlock.AddComponent<MeshRenderer>().material = FindObjectOfType<WorldOfGodcraft>().mat;
        dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(blockID);
        dropedBlock.AddComponent<DropedBlock>();
        dropedBlock.transform.localScale /= 3f;
        dropedBlock.layer = 5;

        var entity = ecsWorld.NewEntity();
        var pool = ecsWorld.GetPool<InventoryItem>();
        pool.Add(entity);
        ref var component = ref pool.Get(entity);
        component.blockID = blockID;
        component.view = dropedBlock;
        component.count = 1;
        component.itemType = ItemType.Block;

        return new() { view = dropedBlock, entity = entity, count = component.count };
    }

    // HOT FIX
    private void UpdateDragItem()
    {
        var craftCell = craftInventory.Cells.Find(c => c.EntityItem == dragItem.entity);
        if (craftCell != null)
        {
            ref var component = ref poolItems.Get(craftCell.EntityItem.Value);
            craftCell.UpdateItem(ref component);
        }

        var cell = quickInventory.Cells.Find(c => c.EntityItem == dragItem.entity);
        if (cell != null)
        {
            ref var component = ref poolItems.Get(cell.EntityItem.Value);
            cell.UpdateItem(ref component);
        }
        else
        {
            cell = cells.Find(c => c.EntityItem == dragItem.entity);
            if (cell != null)
            {
                ref var component = ref poolItems.Get(cell.EntityItem.Value);
                cell.UpdateItem(ref component);
            }
        }
    }

    
}

public class DragItem
{
    public GameObject view;
    public int entity;
    public int count;
}
