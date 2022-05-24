using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class LifetimeSystem : IEcsRunSystem
{
    [EcsFilter(typeof(LifetimeComponent))]
    readonly EcsFilter filterLifetime = default;

    [EcsPool]
    readonly EcsPool<LifetimeComponent> poolLifetime = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterLifetime)
        {
            ref var lifetime = ref poolLifetime.Get(entity);
            lifetime.value += Time.deltaTime;
        }
    }
}
