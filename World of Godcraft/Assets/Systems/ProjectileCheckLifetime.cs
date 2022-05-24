using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class ProjectileCheckLifetime : IEcsRunSystem
{
    [EcsFilter(typeof(ProjectileComponent))]
    readonly EcsFilter filterProjectiles = default;
    [EcsPool]
    readonly EcsPool<LifetimeComponent> poolLifetime = default;
    [EcsPool]
    readonly EcsPool<ProjectileComponent> poolProjectiles = default;


    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterProjectiles)
        {
            ref var lifetime = ref poolLifetime.Get(entity);
            if(lifetime.value > 3)
            {
                ref var projectile = ref poolProjectiles.Get(entity);
                Object.Destroy(projectile.view);
                systems.GetWorld().DelEntity(entity);
            }
        }
    }
}
