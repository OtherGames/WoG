using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EndFramePhysicsSystem))]
[UpdateAfter(typeof(CameraTransformSystem))]
public class CameraRaycastSystem : SystemBase
{
    CameraTransformSystem cameraTransform;
    BuildPhysicsWorld buildPhysicsWorld;
    EndFramePhysicsSystem endFramePhysicsSystem;
    EndFixedStepSimulationEntityCommandBufferSystem fixedCommandBuffer;

    protected override void OnCreate()
    {
        cameraTransform = World.GetExistingSystem<CameraTransformSystem>();
        buildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
        endFramePhysicsSystem = World.GetExistingSystem<EndFramePhysicsSystem>();
        fixedCommandBuffer = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        float3 cameraPosition = cameraTransform.Position;
        quaternion cameraRotation = cameraTransform.Rotation;
        NativeArray<RigidBody> bodies = buildPhysicsWorld.PhysicsWorld.Bodies;
        Entity hitEntity;

        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        EntityCommandBuffer commandBuffer = fixedCommandBuffer.CreateCommandBuffer();
        Dependency = JobHandle.CombineDependencies(Dependency, endFramePhysicsSystem.GetOutputDependency());

        Entities.ForEach((ref Translation position, ref Rotation rotation, in Hit hitObjecct) =>
        {
            int raycastLength = 5;
            
            var raycastInput = new RaycastInput
            {
                Start = cameraPosition,
                End = cameraPosition + (math.forward(cameraRotation) * raycastLength),
                Filter = CollisionFilter.Default
            };

            var collector = new IgnoreTransparentClosestHitCollector(collisionWorld);

            collisionWorld.CastRay(raycastInput, ref collector);

            var hit = collector.ClosestHit;
            var hitDistance = raycastLength * hit.Fraction;

            position.Value = hit.Position;

            hitEntity = bodies[hit.RigidBodyIndex].Entity;
            

        }).Run();

        fixedCommandBuffer.AddJobHandleForProducer(Dependency);

        //Debug.Log(hitEntity);
    }
}
