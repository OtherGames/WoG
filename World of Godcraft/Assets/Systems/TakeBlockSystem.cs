using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class TakeBlockSystem : IEcsRunSystem
{
    [EcsFilter(typeof(BlockTaked))]
    readonly EcsFilter filter = default;

    [EcsWorld]
    readonly EcsWorld world = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filter)
        {
            var pool = world.GetPool<BlockTaked>();
            ref var component = ref pool.Get(entity);

            var e = world.NewEntity();
            var poolInventory = world.GetPool<InventoryItem>();
            poolInventory.Add(e);
            ref var item = ref poolInventory.Get(e);
            item.itemType = ItemType.Block;
            item.blockID = component.blockID;
            item.view = component.view;
        }
    }
}
