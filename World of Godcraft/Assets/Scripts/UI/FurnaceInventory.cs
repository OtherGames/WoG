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

    public override void Init()
    {
        base.Init();

        poolItems = ecsWorld.GetPool<InventoryItem>();
        craft = Service<Craft>.Get();
        prefabs = Service<PrefabsHolder>.Get();
    }

    protected override void Crafting()
    {
        byte? fireTime = null;

        if(cellCombustible.EntityItem != null)
        {
            ref var item = ref poolItems.Get(cellCombustible.EntityItem.Value);
            if (craft.setsCombustible.ContainsKey(item.blockID))
            {
                fireTime = craft.setsCombustible[item.blockID];
            }
        }

        if(fireTime != null && cellFurnaceable.EntityItem != null)
        {
            ref var item = ref poolItems.Get(cellFurnaceable.EntityItem.Value);

            if (craft.setsFurnaceable.ContainsKey(item.blockID))
            {
                var result = craft.setsFurnaceable[item.blockID];

                CreateItem(result.itemID);
            }
        }
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
