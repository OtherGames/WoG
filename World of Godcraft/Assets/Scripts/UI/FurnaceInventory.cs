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

    private void Update()
    {
        int e = GetEntityFurnace();
        if (e < 0)
            return;

        ref var furnace = ref poolFurnaces.Get(e);

        if(furnace.combustible != null)
        {
            var item = furnace.combustible.Value;
            cellCombustible.UpdateItem(ref item);
        }
        else if(cellCombustible.EntityItem != null)
        {
            ecsWorld.DelEntity(cellCombustible.EntityItem.Value);
            cellCombustible.Clear();
        }

        if (furnace.furnaceable != null)
        {
            var item = furnace.furnaceable.Value;
            cellFurnaceable.UpdateItem(ref item);
        }
        else if (cellFurnaceable.EntityItem != null)
        {
            ecsWorld.DelEntity(cellFurnaceable.EntityItem.Value);
            cellFurnaceable.Clear();
        }

        if (furnace.craftResult != null)
        {
            var crafted = furnace.craftResult.Value;
            if (CellResult.EntityItem == null)
            {
                var entityResult = ecsWorld.NewEntity();
                ref var item = ref poolItems.Add(entityResult);
                item = furnace.craftResult.Value;
                CellResult.Init(entityResult, ref item);
            }
            
            CellResult.UpdateItem(ref crafted);
        }
        else if (CellResult.EntityItem != null)
        {
            ecsWorld.DelEntity(CellResult.EntityItem.Value);
            CellResult.Clear();
        }
    }


    public override void OnShow(Vector3Int posBlock)
    {
        base.OnShow(posBlock);

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

        if (furnace.craftResult != null)
        {
            var entityResult = ecsWorld.NewEntity();
            ref var itemResult = ref poolItems.Add(entityResult);
            itemResult = furnace.craftResult.Value;
            itemResult.view.SetActive(true);
            poolStandalone.Add(entityResult);
            furnace.craftResult = itemResult;

            CellResult.Init(entityResult, ref itemResult);
        }
    }

    public override void OnHide()
    {
        base.OnHide();

        int e = GetEntityFurnace();
        if (e < 0)
            return;

        ref var furnace = ref poolFurnaces.Get(e);

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

        if (CellResult.EntityItem != null)
        {
            ecsWorld.DelEntity(CellResult.EntityItem.Value);
            furnace.craftResult.Value.view.SetActive(false);
            CellResult.Clear();
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
    
    
}
