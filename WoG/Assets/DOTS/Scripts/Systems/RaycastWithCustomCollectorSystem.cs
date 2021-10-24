using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class RaycastWithCustomCollectorSystem : SystemBase
{
    BuildPhysicsWorld buildPhysicsWorld;
    EndFramePhysicsSystem endFramePhysicsSystem;
    EndFixedStepSimulationEntityCommandBufferSystem fixedCommandBuffer;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
        endFramePhysicsSystem = World.GetExistingSystem<EndFramePhysicsSystem>();
        fixedCommandBuffer = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        EntityCommandBuffer commandBuffer = fixedCommandBuffer.CreateCommandBuffer();
        Dependency = JobHandle.CombineDependencies(Dependency, endFramePhysicsSystem.GetOutputDependency());

        Entities.WithoutBurst().ForEach((Entity entity, ref Translation position, ref Rotation rotation, ref Ebanina ebanina) =>
        {
            var raycastLength = 15;

            var raycastInput = new RaycastInput
            {
                Start = position.Value,
                End = position.Value + (math.forward(rotation.Value) * raycastLength),
                Filter = CollisionFilter.Default
            };

            var collector = new IgnoreTransparentClosestHitCollector(collisionWorld);

            collisionWorld.CastRay(raycastInput, ref collector);

            var hit = collector.ClosestHit;
            var hitDistance = raycastLength * hit.Fraction;


            //UnityEngine.Debug.Log($"{collector.NumHits}");
            //UnityEngine.Debug.Log($"{math.forward(rotation.Value)}");
            //UnityEngine.Debug.Log($"{hitDistance}");

            var ebaloNaNol = UnityEngine.Object.FindObjectOfType<BesachayaHyinya>();
            if (!ebaloNaNol)
            {
                var ebaloInstance = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cylinder);
                ebaloInstance.AddComponent<BesachayaHyinya>();
                ebaloInstance.transform.position = hit.Position;//position.Value + (math.forward(rotation.Value) * hitDistance);
                //UnityEngine.Debug.Log(ebaloInstance);
            }


        }).Run();//Schedule();

        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     float deltaTime = Time.DeltaTime;

        // This declares a new kind of job, which is a unit of work to do.
        // The job is declared as an Entities.ForEach with the target components as parameters,
        // meaning it will process all entities in the world that have both
        // Translation and Rotation components. Change it to process the component
        // types you want.
        
        
        
        Entities.ForEach((ref Translation translation, in Rotation rotation) => {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as 'in', which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
        }).Schedule();
    }
}

public struct IgnoreTransparentClosestHitCollector : ICollector<RaycastHit>
{
    public bool EarlyOutOnFirstHit => false;

    public float MaxFraction { get; private set; }

    public int NumHits { get; private set; }

    public RaycastHit ClosestHit;

    private CollisionWorld m_World;
    private const int k_TransparentCustomTag = (1 << 1);

    public IgnoreTransparentClosestHitCollector(CollisionWorld world)
    {
        m_World = world;

        MaxFraction = 1.0f;
        ClosestHit = default;
        NumHits = 0;
    }

    private static bool IsTransparent(BlobAssetReference<Collider> collider, ColliderKey key)
    {
        bool bIsTransparent = false;
        unsafe
        {
            // Only Convex Colliders have Materials associated with them. So base on CollisionType
            // we'll need to cast from the base Collider type, hence, we need the pointer.
            var c = (Collider*)collider.GetUnsafePtr();
            {
                var cc = ((ConvexCollider*)c);

                // We also need to check if our Collider is Composite (i.e. has children).
                // If it is then we grab the actual leaf node hit by the ray.
                // Checking if our collider is composite
                if (c->CollisionType != CollisionType.Convex)
                {
                    // If it is, get the leaf as a Convex Collider
                    c->GetLeaf(key, out ChildCollider child);
                    cc = (ConvexCollider*)child.Collider;
                }

                // Now we've definitely got a ConvexCollider so can check the Material.
                bIsTransparent = (cc->Material.CustomTags & k_TransparentCustomTag) != 0;
            }
        }

        return bIsTransparent;
    }

    public bool AddHit(RaycastHit hit)
    {
        //if (IsTransparent(m_World.Bodies[hit.RigidBodyIndex].Collider, hit.ColliderKey))
        //{
        //    return false;
        //}

        MaxFraction = hit.Fraction;
        ClosestHit = hit;
        NumHits = 1;

        return true;
    }
}
