using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class PlacedFurnaceSystem : IEcsRunSystem
{
    [EcsFilter(typeof(BlockPlaced))]
    readonly EcsFilter filterPlaced = default;
    [EcsPool]
    readonly EcsPool<BlockPlaced> poolPlaced = default;
    [EcsPool]
    readonly EcsPool<FurnaceComponent> poolFurnace = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterPlaced)
        {
            ref var placed = ref poolPlaced.Get(entity);
            if (placed.ID == BLOCKS.FURNACE)
            {
                var e = systems.GetWorld().NewEntity();
                ref var furnace = ref poolFurnace.Add(e);
                furnace.pos = placed.pos;
            }
        }
    }
}
