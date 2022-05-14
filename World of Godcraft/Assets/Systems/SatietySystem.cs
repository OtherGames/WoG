using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class SatietySystem : IEcsRunSystem
{
    [EcsFilter(typeof(Character), typeof(SatietyComponent))]
    readonly EcsFilter filter = default;

    [EcsPool]
    readonly EcsPool<SatietyComponent> pool = default;

    float decreaseTimer = 0;

    public void Run(EcsSystems systems)
    {
        decreaseTimer += Time.deltaTime;

        if (decreaseTimer < 10)
            return;

        foreach (var entity in filter)
        {
            ref var satiety = ref pool.Get(entity);

            if (satiety.Value > 0)
                satiety.Value--;

            decreaseTimer = 0;
        }
    }
}
