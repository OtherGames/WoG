using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class TakeBlockSystem : IEcsRunSystem
{
    [EcsFilter(typeof(BlockTaked))]
    readonly EcsFilter filter = default;

    [EcsFilter(typeof(InventoryItem))]
    readonly EcsFilter filterInventory = default;

    [EcsFilter(typeof(InventoryItem), typeof(ItemQuickInventory))]
    readonly EcsFilter filterQuickInventory = default;

    [EcsWorld]
    readonly EcsWorld world = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filter)
        {
            var pool = world.GetPool<BlockTaked>();
            ref var component = ref pool.Get(entity);

            var poolInventory = world.GetPool<InventoryItem>();

            bool isNewItem = true;

            //foreach (var entityInventory in filterQuickInventory)
            //{
            //    var poolQuickInventory = world.GetPool<ItemQuickInventory>();
            //    ref var itemQuick = ref poolInventory.Get(entityInventory);

            //    if(itemQuick.blockID == component.blockID)
            //    {
            //        itemQuick.count++;
            //        isNewItem = false;

            //        Object.Destroy(itemQuick.view);
            //    }
            //}
            
            foreach (var entityInventory in filterInventory)
            {
                ref var itemInventory = ref poolInventory.Get(entityInventory);
                if(itemInventory.blockID == component.blockID)
                {
                    itemInventory.count++;
                    isNewItem = false;

                    Object.Destroy(component.view);
                }
            }

            if (isNewItem)
            {
                TakeAsNewItem(poolInventory, ref component);
            }

            GlobalEvents.itemTaked?.Invoke();
        }
    }


    void TakeAsNewItem(EcsPool<InventoryItem> pool, ref BlockTaked component)
    {
        var e = world.NewEntity();
        pool.Add(e);
        ref var item = ref pool.Get(e);
        item.itemType = ItemType.Block;
        item.blockID = component.blockID;
        item.view = component.view;
        item.count++;

        if (filterQuickInventory.GetEntitiesCount() < 10)
        {
            var poolQuickInventory = world.GetPool<ItemQuickInventory>();
            poolQuickInventory.Add(e);
        }
    }
}
