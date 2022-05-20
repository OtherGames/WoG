using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class GunFireRateSystem : IEcsRunSystem
{
    [EcsFilter(typeof(GunComponent))]
    readonly EcsFilter filterGuns = default;
    [EcsPool]
    readonly EcsPool<GunComponent> poolGuns = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterGuns)
        {
            ref var gun = ref poolGuns.Get(entity);
            gun.fireTime += Time.deltaTime;

            if (gun.fireTime > gun.fireRate)
            {
                gun.shotAvailable = true;
            }
        }
    }
}
