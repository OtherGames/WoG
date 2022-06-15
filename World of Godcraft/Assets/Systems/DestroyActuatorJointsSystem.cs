using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class DestroyActuatorJointsSystem : IEcsRunSystem
{
    [EcsFilter(typeof(EngineMined))]
    readonly EcsFilter filterMined = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterMined)
        {
            Debug.Log("уеба");
        }
    }
}
