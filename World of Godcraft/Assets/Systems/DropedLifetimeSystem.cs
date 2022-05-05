using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

sealed class DropedLifetimeSystem : IEcsRunSystem
{
    [EcsFilter(typeof(DropedComponent))]
    readonly EcsFilter filter = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filter)
        {
            var pool = systems.GetWorld().GetPool<DropedComponent>();
            ref var component = ref pool.Get(entity);
            component.lifetime += UnityEngine.Time.deltaTime;
        }
    }
}
