using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using UnityStandardAssets.CrossPlatformInput;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ConvertToEntitySystem))]
public class PlayerMovementSystem : ComponentSystem
{
    float3 moveDir;

    //===== ROTATE ======
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;

    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;

    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth = true;
    public float smoothTime = 50;
    //public bool lockCursor = true;

    bool kostyl;

    protected override void OnCreate()
    {
        base.OnCreate();

        Entities.ForEach((ref PlayerComponent player, ref LocalToWorld transform) =>
        {
            m_CharacterTargetRot = transform.Rotation;
            m_CameraTargetRot = Camera.main.transform.localRotation;
            Debug.Log(m_CameraTargetRot+ " -+-+-+-+-");
        });

        //Debug.Log(m_CameraTargetRot + " -+-+-+-+-");
    }


    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        float2 input = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        

        Entities.ForEach((Entity entity, ref PlayerComponent player, ref LocalToWorld transform, ref PhysicsVelocity velocity, ref Rotation rot, ref PhysicsMass mass) =>
        {
            float3 desiredMove = transform.Forward * input.y + transform.Right * input.x;
            // TODO

            moveDir.x = desiredMove.x * player.movementSpeed;
            moveDir.z = desiredMove.z * player.movementSpeed;
            //moveDir.y = -1;

            moveDir *= Time.DeltaTime;

            velocity.Linear = new float3(moveDir.x, velocity.Linear.y, moveDir.z);
            //velocity.Angular = new float3(0, input.x * player.rotationSpeed * deltaTime, 0);

            //===============================================================
            if (!kostyl)
            {
                m_CharacterTargetRot = rot.Value;
                m_CameraTargetRot = Camera.main.transform.localRotation;
                kostyl = true;
            }
            //-----------------------------
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;
            //Debug.Log(yRot + " - " + xRot);
            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
            
            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            //Transform character = EntityManager.GetComponentObject<Transform>(player.cameraTarget);
            Transform camera = Camera.main.transform;

            
            float3 Gravity = new float3(0, -9.18f, 0);
            var up = math.select(math.up(), -math.normalize(Gravity),
                math.lengthsq(Gravity) > 0f);

            bool haveInput = (math.abs(yRot) > float.Epsilon);
            if (haveInput)
            {
                var userRotationSpeed = yRot * 3;
                //velocity.Angular = -userRotationSpeed * up;

                velocity.Angular = Vector3.Slerp(velocity.Angular, -userRotationSpeed * up, 10);
                //ccInternalData.CurrentRotationAngle += userRotationSpeed * DeltaTime;
            }
            else
            {
                velocity.Angular = 0;
            }

            //mass.InertiaOrientation = Quaternion.Slerp(mass.InertiaOrientation, m_CharacterTargetRot,
            //    smoothTime * Time.DeltaTime);

            camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                smoothTime * Time.DeltaTime);


            //======================================
            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.Linear = new float3(velocity.Linear.x, 10, velocity.Linear.z);
            }
        });
    }

    quaternion ClampRotationAroundXAxis(quaternion q)
    {
        q.value.x /= q.value.w;
        q.value.y /= q.value.w;
        q.value.z /= q.value.w;
        q.value.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.value.x);

        angleX = math.clamp(angleX, MinimumX, MaximumX);

        q.value.x = math.tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}
