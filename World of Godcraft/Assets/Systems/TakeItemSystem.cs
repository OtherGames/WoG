using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;


sealed class TakeItemSystem : IEcsRunSystem
{
    [EcsWorld]
    readonly EcsWorld ecsWorld = default;
    [EcsPool]
    readonly EcsPool<ItemTaked> poolTaked = default;
    [EcsPool]
    readonly EcsPool<PickableComponent> poolPickable = default;
    [EcsPool]
    readonly EcsPool<InventoryItem> poolInventory = default;
    [EcsFilter(typeof(ItemTaked))]
    readonly EcsFilter filterTaked = default;
    [EcsFilter(typeof(PickableComponent))]
    readonly EcsFilter filterPickable = default;
    [EcsFilter(typeof(InventoryItem))]
    readonly EcsFilter filterInventory = default;
    [EcsFilter(typeof(InventoryItem), typeof(ItemQuickInventory))]
    readonly EcsFilter filterQuickInventory = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterTaked)
        {
            ref var taked = ref poolTaked.Get(entity);

            foreach (var entityPickable in filterPickable)
            {
                ref var pickable = ref poolPickable.Get(entityPickable);
                if (pickable.view == taked.view)
                {
                    bool isNewItem = true;

                    foreach (var entityInventory in filterInventory)
                    {
                        ref var itemInventory = ref poolInventory.Get(entityInventory);
                        if (itemInventory.blockID == pickable.id)
                        {
                            itemInventory.count++;
                            isNewItem = false;

                            Object.Destroy(pickable.view);
                        }
                    }

                    if (isNewItem)
                    {
                        TakeAsNewItem(ref pickable, entityPickable);
                    }

                    GlobalEvents.itemTaked?.Invoke();
                }
            }
        }
    }

    void TakeAsNewItem(ref PickableComponent pickable, int entity)
    {
        ref var inventory = ref poolInventory.Add(entity);
        inventory.blockID = pickable.id;
        inventory.count++;
        inventory.itemType = ItemType.Item;
        inventory.view = pickable.view;

        if (filterQuickInventory.GetEntitiesCount() < 10)
        {
            ecsWorld.GetPool<ItemQuickInventory>().Add(entity);
        }
    }
}
