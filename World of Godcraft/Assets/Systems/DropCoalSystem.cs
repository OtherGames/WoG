using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class DropCoalSystem : IEcsRunSystem
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

            if (droped.BlockID == 6)
            {
                droped.BlockID = ITEMS.COAL;

                var pos = droped.view.transform.position;

                Object.Destroy(droped.view);

                var coal = Object.Instantiate(prefabs.coal, pos, Quaternion.identity);
                coal.AddComponent<DropedBlock>();

                droped.view = coal;
                droped.itemType = ItemType.Item;
            }
        }
    }
}
