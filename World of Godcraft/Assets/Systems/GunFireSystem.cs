using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class GunFireSystem : IEcsRunSystem
{
    [EcsPool]
    readonly EcsPool<Character> poolPlayers = default;
    [EcsPool]
    readonly EcsPool<InventoryItem> poolItems = default;
    [EcsPool]
    readonly EcsPool<GunComponent> poolGuns = default;
    [EcsPool]
    readonly EcsPool<ProjectileComponent> poolProjectiles = default;    
    //[EcsPool]
    //readonly EcsPool<DropedLifetimeSystem>

    [EcsFilter(typeof(GunFired))]
    readonly EcsFilter filterFired = default;
    [EcsFilter(typeof(InventoryItem))]
    readonly EcsFilter filterInventory = default;
    [EcsFilter(typeof(Character))]
    readonly EcsFilter filterPlayer = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterFired)
        {
            ref var gun = ref poolGuns.Get(entity);
            gun.fireTime = 0;
            gun.shotAvailable = false;

            foreach (var entityItem in filterInventory)
            {
                ref var item = ref poolItems.Get(entityItem);
                if (item.blockID == ITEMS.BULLET)
                {
                    Transform parent = null;

                    foreach (var ePlayer in filterPlayer)
                    {
                        ref var player = ref poolPlayers.Get(ePlayer);
                        parent = player.view.GunView?.muzzle;
                    }

                    var view = Object.Instantiate(item.view);
                    int entityProjectile = systems.GetWorld().NewEntity();
                    ref var projectile = ref poolProjectiles.Add(entityProjectile);
                    projectile.view = view;
                    projectile.speed = 15f;
                    projectile.dir = parent.transform.forward;
                    view.transform.position = parent.position;
                    view.transform.forward = parent.transform.forward;
                    view.layer = 0;
                }
            }
        }
    }
}
