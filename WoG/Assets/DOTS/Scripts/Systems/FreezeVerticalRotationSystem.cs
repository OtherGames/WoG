using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Jobs;

public class FreezeVerticalRotationSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle job = Entities.ForEach((ref FreezeVerticalRotationComponent tag, ref PhysicsMass physics) => 
        {
            physics.InverseInertia.x = 0;
            physics.InverseInertia.z = 0;
        }).Schedule(inputDeps);

        return job;
    }
}
