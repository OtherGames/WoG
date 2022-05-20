using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class ProjectileSystem : IEcsRunSystem
{
    [EcsFilter(typeof(ProjectileComponent))]
    EcsFilter filterProjectile = default;
    [EcsPool]
    EcsPool<ProjectileComponent> poolProjectiles = default;

    public void Run(EcsSystems systems)
    {
        foreach (var e in filterProjectile)
        {
            ref var projectile = ref poolProjectiles.Get(e);
            var dir = projectile.dir;
            projectile.view.transform.position += dir * projectile.speed * Time.deltaTime;
        }
    }
}
