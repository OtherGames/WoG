using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

sealed class TakeDropedSystem : IEcsRunSystem
{
    [EcsFilter(typeof(DropedComponent))]
    readonly EcsFilter filter = default;

    [EcsWorld]
    readonly EcsWorld world = default;

    PlayerCharacter player;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filter)
        {
            if (!player)
                player = Object.FindObjectOfType<PlayerCharacter>();

            var pool = systems.GetWorld().GetPool<DropedComponent>();
            ref var component = ref pool.Get(entity);

            if(component.lifetime > 1f)
            {
                var dist = Vector3.Distance(player.transform.position, component.view.transform.position);
                
                if(dist < 3)
                {
                    var dir = player.transform.position - component.view.transform.position;
                    dir.Normalize();

                    if(dist < 0.18f)
                    {
                        TakeBlock(ref component);
                        pool.Del(entity);
                    }
                    else 
                    {
                        component.view.transform.position += (5 / dist) * Time.deltaTime * dir;
                    }
                }
            }
        }
    }

    void TakeBlock(ref DropedComponent component)
    {
        var e = world.NewEntity();
        var poolTaked = world.GetPool<BlockTaked>();
        poolTaked.Add(e);
        ref var taked = ref poolTaked.Get(e);
        taked.blockID = component.BlockID;
        taked.view = component.view;
        taked.itemType = component.itemType;

        component.view.SetActive(false);
    }
}
