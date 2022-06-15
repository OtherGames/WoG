using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class DestroyActuatorJointsSystem : IEcsRunSystem
{
    [EcsFilter(typeof(EngineMined))]
    readonly EcsFilter filterMined = default;
    [EcsFilter(typeof(EngineActuatorConnectionComponent))]
    readonly EcsFilter filterConnections = default;

    [EcsPool]
    readonly EcsPool<EngineMined> poolMined = default;
    [EcsPool]
    readonly EcsPool<EngineActuatorConnectionComponent> poolConnections = default;

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterMined)
        {
            foreach (var eConnection in filterConnections)
            {
                ref var mined = ref poolMined.Get(entity);
                ref var connection = ref poolConnections.Get(eConnection);

                if (mined.bodyView == connection.bodyView)
                {
                    if(mined.enginePos == connection.engineBlockPos)
                    {
                        Object.Destroy(connection.joint);
                    }
                }
            }
        }
    }
}
