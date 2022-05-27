using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;
using System.Collections.Generic;

sealed class VehicleHitSystem : IEcsRunSystem
{
    [EcsFilter(typeof(VehicleHitEvent))]
    readonly EcsFilter filterHitEvent = default;
    [EcsFilter(typeof(VehicleComponent))]
    readonly EcsFilter filterVehicle = default;
    [EcsPool]
    readonly EcsPool<VehicleComponent> poolVehicle = default;
    [EcsPool]
    readonly EcsPool<VehicleHitEvent> poolHitEvent = default;
    readonly MeshGenerator meshGenerator = Service<MeshGenerator>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterHitEvent)
        {
            ref var hitEvent = ref poolHitEvent.Get(entity);
            Debug.Log(hitEvent.blockPos);

            foreach (var entityVehicle in filterVehicle)
            {
                ref var vehicle = ref poolVehicle.Get(entityVehicle);
                var pos = hitEvent.blockPos;

                if (hitEvent.blockID > 0)
                {
                    var connectedID = GetConnectedID(ref vehicle, ref hitEvent);
                    
                    if(connectedID == BLOCKS.ENGINE && hitEvent.blockID == BLOCKS.ACTUATOR)
                    {
                        Debug.Log("Туби пязда");

                        ActuatorAttach(ref vehicle, ref hitEvent);

                        return;
                    }

                    if (pos.x >= vehicle.size + vehicle.meshOffset.x || pos.y >= vehicle.size + vehicle.meshOffset.y || pos.z >= vehicle.size + vehicle.meshOffset.z)
                    {
                        EnlargementBlocksSize(ref vehicle);
                    }
                    if (pos.x < 0)
                    {
                        EnlargementBlocksSize(ref vehicle);

                        ShiftBlocksLeft(ref vehicle);
                    }
                    if (pos.y < 0)
                    {
                        EnlargementBlocksSize(ref vehicle);

                        ShiftBlocksDown(ref vehicle);
                    }
                    if (pos.z < 0)
                    {
                        EnlargementBlocksSize(ref vehicle);

                        ShiftBlocksBack(ref vehicle);
                    }

                    Debug.Log("Offset: " + vehicle.meshOffset);
                    int x = pos.x - Mathf.RoundToInt(vehicle.meshOffset.x);
                    int y = pos.y - Mathf.RoundToInt(vehicle.meshOffset.y);
                    int z = pos.z - Mathf.RoundToInt(vehicle.meshOffset.z);
                    vehicle.blocks[x][y][z] = hitEvent.blockID;

                    var mesh = meshGenerator.UpdateVehicleMesh(ref vehicle);
                    vehicle.meshFilter.mesh = mesh;
                    var collider = new GameObject("Collider");
                    var component = collider.AddComponent<BoxCollider>();
                    component.center += new Vector3(-0.5f, 0.5f, 0.5f);
                    collider.layer = LayerMask.NameToLayer("Vehicle");
                    collider.transform.parent = vehicle.renderer.transform;
                    collider.transform.localPosition = pos;
                    collider.transform.localRotation = Quaternion.identity;
                    vehicle.colliders.Add(pos, collider);
                }
                else
                {
                    int x = pos.x - Mathf.RoundToInt(vehicle.meshOffset.x);
                    int y = pos.y - Mathf.RoundToInt(vehicle.meshOffset.y);
                    int z = pos.z - Mathf.RoundToInt(vehicle.meshOffset.z);
                    vehicle.blocks[x][y][z] = hitEvent.blockID;

                    var mesh = meshGenerator.UpdateVehicleMesh(ref vehicle);
                    vehicle.meshFilter.mesh = mesh;

                    var collider = vehicle.colliders[pos];
                    vehicle.colliders.Remove(pos);
                    Object.Destroy(collider);
                }
            }
        }
    }

    void EnlargementBlocksSize(ref VehicleComponent vehicle)
    {
        vehicle.size++;
        vehicle.blocks.Add(new());

        for (int i = 0; i < vehicle.size; i++)
        {
            while (vehicle.blocks[i].Count < vehicle.size)
            {
                vehicle.blocks[i].Add(new());
            }
        }

        for (int i = 0; i < vehicle.size; i++)
        {
            for (int j = 0; j < vehicle.size; j++)
            {
                while (vehicle.blocks[i][j].Count < vehicle.size)
                {
                    vehicle.blocks[i][j].Add(0);
                }
            }
        }
    }

    void ShiftBlocksBack(ref VehicleComponent vehicle)
    {
        //List<List<List<byte>>> blocksCopy = new();
        //blocksCopy.AddRange(vehicle.blocks);

        for (int i = vehicle.size - 1; i >= 0; i--)
        {
            for (int j = vehicle.size - 1; j >= 0; j--)
            {
                for (int z = vehicle.size - 1; z >= 0; z--)
                {
                    if (z - 1 < 0)
                    {
                        //blocksCopy[i][j][z] = 0;
                        vehicle.blocks[i][j][z] = 0;
                    }
                    else
                    {
                        //blocksCopy[i][j][z] = vehicle.blocks[i][j][z - 1];
                        vehicle.blocks[i][j][z] = vehicle.blocks[i][j][z - 1];
                    }
                }
            }
        }

        //vehicle.blocks = blocksCopy;
        vehicle.meshOffset += Vector3.back;
    }

    void ShiftBlocksLeft(ref VehicleComponent vehicle)
    {
        List<List<List<byte>>> blocksCopy = new();
        blocksCopy.AddRange(vehicle.blocks);

        for (int x = vehicle.size - 1; x >= 0; x--)
        {
            for (int j = vehicle.size - 1; j >= 0; j--)
            {
                for (int z = vehicle.size - 1; z >= 0; z--)
                {
                    if (x - 1 < 0)
                    {
                        blocksCopy[x][j][z] = 0;
                    }
                    else
                    {
                        blocksCopy[x][j][z] = vehicle.blocks[x - 1][j][z];
                    }
                }
            }
        }

        vehicle.blocks = blocksCopy;
        vehicle.meshOffset += Vector3.left;
    }

    void ShiftBlocksDown(ref VehicleComponent vehicle)
    {
        List<List<List<byte>>> blocksCopy = new();
        blocksCopy.AddRange(vehicle.blocks);

        for (int x = vehicle.size - 1; x >= 0; x--)
        {
            for (int y = vehicle.size - 1; y >= 0; y--)
            {
                for (int z = vehicle.size - 1; z >= 0; z--)
                {
                    if (y - 1 < 0)
                    {
                        blocksCopy[x][y][z] = 0;
                    }
                    else
                    {
                        blocksCopy[x][y][z] = vehicle.blocks[x][y - 1][z];
                    }
                }
            }
        }

        vehicle.blocks = blocksCopy;
        vehicle.meshOffset += Vector3.down;
    }

    void ActuatorAttach(ref VehicleComponent vehicle, ref VehicleHitEvent hitEvent)
    {
        var actuator = new GameObject("Actuator");

        actuator.transform.parent = vehicle.renderer.transform.parent;
        var body = actuator.AddComponent<Rigidbody>();
        var collider = actuator.AddComponent<BoxCollider>();
        collider.center += new Vector3(-0.5f, 0.5f, 0.5f);
        var layer = LayerMask.NameToLayer("Vehicle");
        actuator.layer = layer;
        var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
        actuator.AddComponent<MeshRenderer>().material = Object.FindObjectOfType<WorldOfGodcraft>().mat;
        actuator.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMeshBlock(hitEvent.blockID);
        var pos = vehicle.renderer.transform.position + hitEvent.blockPos;
        actuator.transform.position = pos;

        var joint = actuator.AddComponent<HingeJoint>();
        joint.connectedBody = vehicle.renderer.GetComponent<Rigidbody>();
        joint.anchor = new(-0.5f, 0.5f, 0.5f);
        joint.axis = hitEvent.blockPos - hitEvent.connectedPos;
    }


    byte GetConnectedID(ref VehicleComponent vehicle, ref VehicleHitEvent hitEvent)
    {
        var connectedPos = hitEvent.connectedPos;
        var x = connectedPos.x - Mathf.RoundToInt(vehicle.meshOffset.x);
        var y = connectedPos.y - Mathf.RoundToInt(vehicle.meshOffset.y);
        var z = connectedPos.z - Mathf.RoundToInt(vehicle.meshOffset.z);

        return vehicle.blocks[x][y][z];
    }
}
