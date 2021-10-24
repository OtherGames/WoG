using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

[UpdateAfter(typeof(EbaninaSystem))]
public class CopyTransformSystem : ComponentSystem
{
    EbaninaSystem ebanina;

    protected override void OnCreate()
    {
        //ebanina = World.CreateSystem
        ebanina = World.GetOrCreateSystem<EbaninaSystem>();
    }

    protected override void OnUpdate()
    {
        //ebanina = World.GetOrCreateSystem<EbaninaSystem>();

        Entities.ForEach((Entity entity, ref CopyTransformComponent tag, ref LocalToWorld localToWorld, ref Rotation rotata) => 
        {
            var transform = EntityManager.GetComponentObject<Transform>(entity);
            //transform.position = localToWorld.Position;

            transform.position = Vector3.Slerp(transform.position, localToWorld.Position, 0.3f);

            //transform.rotation = ebanina.rototos.Value;

            transform.rotation = Quaternion.Slerp(transform.rotation, ebanina.rototos.Value, 0.3f);
        });
    }
}

[UpdateBefore(typeof(CopyTransformSystem))]
//[DisableAutoCreation]
public class EbaninaSystem : ComponentSystem
{
    public RigidTransform transform;
    public Rotation rototos;

    protected override void OnUpdate()
    {
        Entities.ForEach((ref PlayerComponent player, ref Rotation rot, ref PhysicsMass mass) =>
        {
            rototos = rot;
            //transform = mass.Transform;
            //Quaternion q = mass.Transform.rot;
            //Debug.Log(mass.Transform.rot);
        });
    }
}
