using Leopotam.EcsLite;
using LeopotamGroup.Globals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceInventory : CraftInventory
{
    [Space]

    [SerializeField] CellCraftInventory cellCombustible;
    [SerializeField] CellCraftInventory cellFurnaceable;
    
    Craft craft;
    PrefabsHolder prefabs;

    EcsPool<InventoryItem> poolItems;
    EcsPool<FurnaceComponent> poolFurnaces;
    EcsPool<StandaloneInventory> poolStandalone;
    EcsFilter filterFurnace;

    public override void Init()
    {
        base.Init();

        filterFurnace = ecsWorld.Filter<FurnaceComponent>().End();
        poolItems = ecsWorld.GetPool<InventoryItem>();
        poolFurnaces = ecsWorld.GetPool<FurnaceComponent>();
        poolStandalone = ecsWorld.GetPool<StandaloneInventory>();
        craft = Service<Craft>.Get();
        prefabs = Service<PrefabsHolder>.Get();
    }

    protected override void CraftItem_Seted()
    {
        int e = GetEntityFurnace();
        
        ref var furnace = ref poolFurnaces.Get(e);

        foreach (var cell in Cells)
        {
            if (cell.EntityItem != null && !poolStandalone.Has(cell.EntityItem.Value))
            {
                ref var standalone = ref poolStandalone.Add(cell.EntityItem.Value);
                standalone.ownerEntity = e;

                ref var item = ref poolItems.Get(cell.EntityItem.Value);

                if (cell == cellCombustible)
                {
                    furnace.combustible = item;
                }

                if(cell == cellFurnaceable)
                {
                    furnace.furnaceable = item;
                }
            }
        }

    }

    public override void UpdateInventory(Vector3Int posBlock)
    {
        base.UpdateInventory(posBlock);

        int e = GetEntityFurnace();

        ref var furnace = ref poolFurnaces.Get(e);

        if (furnace.combustible != null)
        {
            var entityCombustible = ecsWorld.NewEntity();
            ref var itemCombustible = ref poolItems.Add(entityCombustible);
            itemCombustible = furnace.combustible.Value;
            itemCombustible.view.SetActive(true);
            poolStandalone.Add(entityCombustible);
            furnace.combustible = itemCombustible;

            cellCombustible.Init(entityCombustible, ref itemCombustible);
        }

        if (furnace.furnaceable != null)
        {
            var entityFurnaceable = ecsWorld.NewEntity();
            ref var itemFurnaceable = ref poolItems.Add(entityFurnaceable);
            itemFurnaceable = furnace.furnaceable.Value;
            itemFurnaceable.view.SetActive(true);
            poolStandalone.Add(entityFurnaceable);
            furnace.furnaceable = itemFurnaceable;

            cellFurnaceable.Init(entityFurnaceable, ref itemFurnaceable);
        }
    }

    public override void OnHide()
    {
        base.OnHide();

        int e = GetEntityFurnace();
        if (e < 0)
            return;

        ref var furnace = ref poolFurnaces.Get(e);

        //ref var itemCombustible = ref poolItems.Get(cellCombustible.EntityItem.Value);
        //ref var itemFurnaceable = ref poolItems.Get(cellFurnaceable.EntityItem.Value);
        //furnace.combustible = itemCombustible;
        //furnace.furnaceable = itemFurnaceable;
        if (cellCombustible.EntityItem != null)
        {
            ecsWorld.DelEntity(cellCombustible.EntityItem.Value);
            furnace.combustible.Value.view.SetActive(false);
            cellCombustible.Clear();
        }
        if (cellFurnaceable.EntityItem != null)
        {
            ecsWorld.DelEntity(cellFurnaceable.EntityItem.Value);
            furnace.furnaceable.Value.view.SetActive(false);
            cellFurnaceable.Clear();
        }
    }

    private int GetEntityFurnace()
    {
        int result = -1;

        foreach (var e in filterFurnace)
        {
            ref var furnace = ref poolFurnaces.Get(e);

            if (furnace.pos == posBlock)
            {
                result = e;
            }
        }

        return result;
    }

    protected override void Crafting()
    {
        
    }
    
    void CreateItem(byte id)
    {
        if(id == ITEMS.INGOT_IRON)
        {
            var ingot = Instantiate(prefabs.ingotIron);
            ingot.layer = 5;
            ingot.transform.localScale = Vector3.one * 0.7f;

            var entity = ecsWorld.NewEntity();
            var pool = ecsWorld.GetPool<InventoryItem>();
            pool.Add(entity);
            ref var component = ref pool.Get(entity);
            component.blockID = id;
            component.view = ingot;
            component.count = 1;
            component.itemType = ItemType.Item;
            component.rotation = new (1.327f, 95.58f, -33.715f);

            CellResult.Init(entity, ref component);

            craftedItem = new() { entity = entity, view = ingot };
        }
    }
}
