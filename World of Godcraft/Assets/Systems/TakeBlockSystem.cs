using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class TakeBlockSystem : IEcsRunSystem
{
    [EcsFilter(typeof(BlockTaked))]
    readonly EcsFilter filter = default;

    [EcsFilter(typeof(InventoryItem))]
    readonly EcsFilter filterInventory = default;

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
                var e = world.NewEntity();
                poolInventory.Add(e);
                ref var item = ref poolInventory.Get(e);
                item.itemType = ItemType.Block;
                item.blockID = component.blockID;
                item.view = component.view;
                item.count++;
            }
        }
    }
}
