using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class UsedItemUpdatedSystem : IEcsRunSystem
{
    [EcsFilter(typeof(UsedItemUpdated))]
    EcsFilter filterUpdated = default;

    [EcsPool]
    EcsPool<UsedItemUpdated> poolUpdated = default;


    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterUpdated)
        {
            Debug.Log("ух ептить");
        }
    }
}
