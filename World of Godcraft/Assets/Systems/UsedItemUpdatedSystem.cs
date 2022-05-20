using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class UsedItemUpdatedSystem : IEcsRunSystem
{
    [EcsFilter(typeof(UsedItemUpdated))]
    readonly EcsFilter filterUpdated = default;

    [EcsPool]
    readonly EcsPool<UsedItemUpdated> poolUpdated = default;
    [EcsPool]
    readonly EcsPool<InventoryItem> poolInventory = default;
    [EcsPool]
    readonly EcsPool<GunComponent> poolGuns = default;


    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterUpdated)
        {
            ref var updated = ref poolUpdated.Get(entity);
            if (updated.id != null)
            {
                ref var item = ref poolInventory.Get(updated.entity);

                if(updated.id == ITEMS.SIMPLE_PISTOL)
                {
                    ref var gun = ref poolGuns.Add(updated.entity);
                    gun.fireRate = 0.3f;
                    gun.view = item.view.GetComponent<GunView>();
                }
            }

        }
    }
}
