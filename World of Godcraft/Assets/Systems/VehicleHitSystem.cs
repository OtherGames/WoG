using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;
using System.Collections.Generic;

sealed class VehicleHitSystem : IEcsRunSystem
{
    [EcsWorld]
    readonly EcsWorld ecsWorld = default;

    [EcsFilter(typeof(VehicleHitEvent))]
    readonly EcsFilter filterHitEvent = default;
    [EcsFilter(typeof(VehicleComponent))]
    readonly EcsFilter filterVehicle = default;

    [EcsPool]
    readonly EcsPool<EngineMined> poolEngineMined = default;
    [EcsPool]
    readonly EcsPool<VehicleComponent> poolVehicle = default;
    [EcsPool]
    readonly EcsPool<EngineActuatorConnectionComponent> poolConnection = default;
    [EcsPool]
    readonly EcsPool<VehicleHitEvent> poolHitEvent = default;

    readonly MeshGenerator meshGenerator = Service<MeshGenerator>.Get();
    readonly ActuatorMeshGenerator actuatorMesh = Service<ActuatorMeshGenerator>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterHitEvent)
        {
            ref var hitEvent = ref poolHitEvent.Get(entity);
            Debug.Log($"Block pos {hitEvent.blockPos}");
            //Debug.Log(hitEvent.entityVehicle + " Сущность");
            ref var vehicle = ref poolVehicle.Get(hitEvent.entityVehicle);
            var pos = hitEvent.blockPos;
            var connectedID = GetConnectedID(ref vehicle, ref hitEvent);

            if (hitEvent.blockID > 0)
            {
                if (connectedID == BLOCKS.ENGINE && hitEvent.blockID == BLOCKS.ACTUATOR)
                {
                    //Debug.Log("Туби пязда " + vehicle.renderer);

                    ActuatorAttach(ref vehicle, ref hitEvent);

                    return;
                }

                if (connectedID == BLOCKS.ENGINE && hitEvent.blockID == BLOCKS.ACTUATOR_ROTARY)
                {
                    ActuatorRotaryAttach(ref vehicle, ref hitEvent);

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
                var isActuator = poolConnection.Has(hitEvent.entityVehicle);
                Mesh mesh; 
                if (isActuator)
                {
                    mesh = actuatorMesh.UpdateVehicleMesh(ref vehicle);
                }
                else
                {
                    mesh = meshGenerator.UpdateVehicleMesh(ref vehicle);
                }
                vehicle.meshFilter.mesh = mesh;
                var collider = new GameObject("Collider");
                var component = collider.AddComponent<BoxCollider>();
                if (!isActuator)// HOT FIX
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
                var isActuator = vehicle.renderer.GetComponent<HingeJoint>();
                Mesh mesh;
                if (isActuator)
                {
                    mesh = actuatorMesh.UpdateVehicleMesh(ref vehicle);
                }
                else
                {
                    mesh = meshGenerator.UpdateVehicleMesh(ref vehicle);
                }
                vehicle.meshFilter.mesh = mesh;

                var collider = vehicle.colliders[pos];
                vehicle.colliders.Remove(pos);
                Object.Destroy(collider);

                CreateDropedBlock(connectedID, hitEvent.globalPos.x, hitEvent.globalPos.y, hitEvent.globalPos.z);
            
                if(connectedID == BLOCKS.ENGINE)
                {
                    ref var engineMined = ref poolEngineMined.Add(ecsWorld.NewEntity());
                    engineMined.enginePos = hitEvent.connectedPos;
                    engineMined.bodyView = vehicle.view.gameObject;
                }
            }
        }
    }

    void EnlargementBlocksSize(ref VehicleComponent vehicle)
    {
        vehicle.size++;
        vehicle.blocks.Add(new());
        Debug.Log("Расширение");
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
        var e = ecsWorld.NewEntity();
        ref var component = ref poolVehicle.Add(e);
        component.size = 1;
        component.blocks = new();
        component.blocks.Add(new());
        for (int x = 0; x < component.size; x++)
        {
            component.blocks[x] = new();
            component.blocks[x].Add(new());

            for (int y = 0; y < component.size; y++)
            {
                component.blocks[x][y] = new();
                component.blocks[x][y].Add(new());

                for (int z = 0; z < component.size; z++)
                {
                    component.blocks[x][y][z] = BLOCKS.ACTUATOR;
                }
            }
        }

        var mesh = actuatorMesh.CreateMesh(ref component);//meshGenerator.CreateVehicleMesh(ref component);
        var mat = Object.FindObjectOfType<WorldOfGodcraft>().mat;

        var actuator = new GameObject("Actuator");
        var colliderView = new GameObject("Collider");
        var collider = colliderView.AddComponent<BoxCollider>();
        actuator.transform.parent = vehicle.renderer.transform.parent;
        colliderView.transform.parent = actuator.transform;
        var body = actuator.AddComponent<Rigidbody>();
        //collider.center += new Vector3(-0.5f, 0.5f, 0.5f);
        var layer = LayerMask.NameToLayer("Vehicle");
        actuator.layer = layer;
        colliderView.layer = layer;
        var renderer = actuator.AddComponent<MeshRenderer>();
        renderer.material = mat;
        var meshFilter = actuator.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        var pos = vehicle.renderer.transform.position + hitEvent.blockPos;
        actuator.transform.position = hitEvent.globalPos;
        actuator.transform.rotation = Quaternion.Euler(hitEvent.globalRot);

        var joint = actuator.AddComponent<HingeJoint>();
        joint.connectedBody = vehicle.renderer.GetComponent<Rigidbody>();
        joint.anchor = Vector3.zero;
        joint.axis = hitEvent.blockPos - hitEvent.connectedPos;
        joint.useMotor = true;
        (vehicle.view as VehicleView).actuators.Add(joint);

        actuator.AddComponent<View>().EntityID = e;

        component.renderer = renderer;
        component.meshFilter = meshFilter;
        component.pos = actuator.transform.position;
        component.colliders = new() { { Vector3.zero, colliderView } };

        //int xChunck = hitEvent.connectedPos.x - Mathf.RoundToInt(vehicle.meshOffset.x);
        //int yChunck = hitEvent.connectedPos.y - Mathf.RoundToInt(vehicle.meshOffset.y);
        //int zChunck = hitEvent.connectedPos.z - Mathf.RoundToInt(vehicle.meshOffset.z);
        ref var connecton = ref poolConnection.Add(e);
        connecton.joint = joint;
        //Debug.Log($"{hitEvent.connectedPos.x} ^ {hitEvent.connectedPos.y} ^ {hitEvent.connectedPos.z}");
        connecton.engineBlockPos = hitEvent.connectedPos;
        connecton.bodyView = vehicle.view.gameObject;
    }

    void ActuatorRotaryAttach(ref VehicleComponent vehicle, ref VehicleHitEvent hitEvent)
    {
        var layer = LayerMask.NameToLayer("Vehicle");

        var rotary = new GameObject("Rotary") { layer = layer };
        rotary.transform.parent = vehicle.renderer.transform.parent;
        rotary.transform.position = hitEvent.globalPos;
        rotary.transform.rotation = Quaternion.Euler(hitEvent.globalRot);
        var rotaryJoint = rotary.AddComponent<HingeJoint>();
        rotaryJoint.connectedBody = vehicle.renderer.GetComponent<Rigidbody>();
        rotaryJoint.anchor = Vector3.zero;
        rotaryJoint.axis = Vector3.up;
        rotaryJoint.useLimits = true;
        rotaryJoint.useMotor = true;
        var limits = rotaryJoint.limits;
        limits.min =-38;
        limits.max = 38;
        rotaryJoint.limits = limits;
        var rotaryBody = rotaryJoint.GetComponent<Rigidbody>();
        rotaryBody.mass = .3f;
        var actuator = new GameObject("Actuator") { layer = layer };
        var colliderDummy = new GameObject("Collider") { layer = layer };
        
        actuator.transform.parent = vehicle.renderer.transform.parent;
        actuator.transform.position = hitEvent.globalPos;
        actuator.transform.rotation = Quaternion.Euler(hitEvent.globalRot);
        colliderDummy.transform.parent = actuator.transform;
        colliderDummy.transform.localPosition = Vector3.zero;
        var collider = colliderDummy.AddComponent<BoxCollider>();

        var actuatorJoint = actuator.AddComponent<HingeJoint>();
        actuatorJoint.connectedBody = rotaryBody;
        actuatorJoint.anchor = Vector3.zero;
        actuatorJoint.axis = hitEvent.blockPos - hitEvent.connectedPos;
        actuatorJoint.GetComponent<Rigidbody>().mass = .5f;
        actuatorJoint.useMotor = true;

        (vehicle.view as VehicleView).actuators.Add(actuatorJoint);
        (vehicle.view as VehicleView).rotary.Add(rotaryJoint);

        var e = ecsWorld.NewEntity();
        ref var component = ref poolVehicle.Add(e);
        component.size = 1;
        component.blocks = new();
        component.blocks.Add(new());
        for (int x = 0; x < component.size; x++)
        {
            component.blocks[x] = new();
            component.blocks[x].Add(new());

            for (int y = 0; y < component.size; y++)
            {
                component.blocks[x][y] = new();
                component.blocks[x][y].Add(new());

                for (int z = 0; z < component.size; z++)
                {
                    component.blocks[x][y][z] = hitEvent.blockID;
                }
            }
        }

        var mesh = actuatorMesh.CreateMesh(ref component);//meshGenerator.CreateVehicleMesh(ref component);
        var mat = Object.FindObjectOfType<WorldOfGodcraft>().mat;

        var renderer = actuator.AddComponent<MeshRenderer>();
        renderer.material = mat;
        var meshFilter = actuator.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        var pos = vehicle.renderer.transform.position + hitEvent.blockPos;

        actuator.AddComponent<View>().EntityID = e;

        component.renderer = renderer;
        component.meshFilter = meshFilter;
        component.pos = actuator.transform.position;
        component.colliders = new() { { Vector3.zero, colliderDummy } };
    }

    byte GetConnectedID(ref VehicleComponent vehicle, ref VehicleHitEvent hitEvent)
    {
        var connectedPos = hitEvent.connectedPos;
        var x = connectedPos.x - Mathf.RoundToInt(vehicle.meshOffset.x);
        var y = connectedPos.y - Mathf.RoundToInt(vehicle.meshOffset.y);
        var z = connectedPos.z - Mathf.RoundToInt(vehicle.meshOffset.z);

        return vehicle.blocks[x][y][z];
    }

    void CreateDropedBlock(byte id, float x, float y, float z)
    {
        if (id == 2)
        {
            id = 3;
        }

        var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
        var dropedBlock = new GameObject("Droped Block - " + id);
        dropedBlock.AddComponent<MeshRenderer>().material = Object.FindObjectOfType<WorldOfGodcraft>().mat;
        dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(id);
        dropedBlock.AddComponent<DropedBlock>();
        dropedBlock.transform.localScale /= 3f;

        float offsetRandomX = Random.Range(-0.1f, 0.1f);
        float offsetRandomY = Random.Range(-0.1f, 0.1f);
        float offsetRandomZ = Random.Range(-0.1f, 0.1f);

        dropedBlock.transform.position = new(x - offsetRandomX, y + offsetRandomY, z + offsetRandomZ);

        var entity = ecsWorld.NewEntity();
        var pool = ecsWorld.GetPool<DropedComponent>();
        pool.Add(entity);
        ref var component = ref pool.Get(entity);
        component.BlockID = id;
        component.view = dropedBlock;
        component.itemType = ItemType.Block;

        ecsWorld.GetPool<DropedCreated>().Add(entity);
    }
}
