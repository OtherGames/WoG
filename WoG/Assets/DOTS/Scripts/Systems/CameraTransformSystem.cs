using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class CameraTransformSystem : ComponentSystem
{
    public float3 Position;
    public quaternion Rotation;

    protected override void OnUpdate()
    {
        Position = UnityEngine.Camera.main.transform.position;
        Rotation = UnityEngine.Camera.main.transform.rotation;
    }
}
