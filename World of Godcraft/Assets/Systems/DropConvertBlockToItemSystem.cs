using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class DropConvertBlockToItemSystem : IEcsRunSystem
{
    [EcsPool]
    readonly EcsPool<DropedComponent> poolDroped = default;

    [EcsFilter(typeof(DropedComponent), typeof(DropedCreated))]
    readonly EcsFilter filterDroped = default;

    readonly PrefabsHolder prefabs = Service<PrefabsHolder>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterDroped)
        {
            ref var droped = ref poolDroped.Get(entity);

            switch (droped.BlockID)
            {
                case BLOCKS.ORE_COAL:
                    Drop(ref droped, prefabs.coal, ITEMS.COAL);
                    break;

                case BLOCKS.SALTPETER:
                    Drop(ref droped, prefabs.saltpeter, ITEMS.SALTPETER);
                    break;

                case BLOCKS.ORE_SULFUR:
                    Drop(ref droped, prefabs.sulfur, ITEMS.SULFUR);
                    break;

                case BLOCKS.GRAVEL:
                    if(Random.Range(0, 100) < 30)
                    {
                        Drop(ref droped, prefabs.silicon, ITEMS.SILICON);
                    }
                    break;
            }
        }
    }

    void Drop(ref DropedComponent droped, GameObject prefab, byte id)
    {
        droped.BlockID = id;

        var pos = droped.view.transform.position;

        Object.Destroy(droped.view);

        var coal = Object.Instantiate(prefab, pos, Quaternion.identity);
        coal.AddComponent<DropedBlock>();

        droped.view = coal;
        droped.itemType = ItemType.Item;
    }
}
