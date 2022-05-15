using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;


sealed class UsingFoodSystem : IEcsRunSystem
{
    [EcsFilter(typeof(ItemUsed), typeof(FoodComponent))]
    readonly EcsFilter filterFood = default;
    [EcsFilter(typeof(Character), typeof(SatietyComponent))]// TODO for network
    readonly EcsFilter filterPlayers = default;
    [EcsPool]
    readonly EcsPool<SatietyComponent> poolPlayer = default;
    [EcsPool]
    readonly EcsPool<FoodComponent> poolFood = default;
    [EcsPool]
    readonly EcsPool<InventoryItem> poolInventory = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterFood)
        {
            ref var food = ref poolFood.Get(entity);

            foreach (var entityPlayer in filterPlayers)
            {
                ref var player = ref poolPlayer.Get(entityPlayer);
                player.Value += food.satietyValue;
                player.Value = Mathf.Clamp(player.Value, 0, player.MaxValue);
            }

            ref var item = ref poolInventory.Get(entity);
            item.count--;
            if(item.count == 0)
            {
                Object.Destroy(item.view);
                systems.GetWorld().DelEntity(entity);
            }
        }
    }
}
