using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;
using System.Linq;
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
                Debug.Log("Offset: " + vehicle.meshOffset);

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


                var mesh = meshGenerator.UpdateVehicleMesh(ref vehicle);
                vehicle.meshFilter.mesh = mesh;
                vehicle.collider.sharedMesh = mesh;
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
                    vehicle.blocks[i][j].Add(2);
                }
            }
        }
    }

    void ShiftBlocksBack(ref VehicleComponent vehicle)
    {
        List<List<List<byte>>> blocksCopy = new();
        blocksCopy.AddRange(vehicle.blocks);

        for (int i = vehicle.size - 1; i >= 0; i--)
        {
            for (int j = vehicle.size - 1; j >= 0; j--)
            {
                for (int z = vehicle.size - 1; z >= 0; z--)
                {
                    //Debug.Log(blocksCopy[i][j][z]);
                    if (z - 1 < 0)
                    {
                        blocksCopy[i][j][z] = 2;
                    }
                    else
                    {
                        blocksCopy[i][j][z] = vehicle.blocks[i][j][z - 1];
                    }
                }
            }
        }

        vehicle.blocks = blocksCopy;
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
                    //Debug.Log(blocksCopy[i][j][z]);
                    if (x - 1 < 0)
                    {
                        blocksCopy[x][j][z] = 2;
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
                    //Debug.Log(blocksCopy[i][j][z]);
                    if (y - 1 < 0)
                    {
                        blocksCopy[x][y][z] = 2;
                    }
                    else
                    {
                        blocksCopy[x][y][z] = vehicle.blocks[x][y - 1][z];
                    }
                }
            }
        }

        vehicle.blocks = blocksCopy;
        vehicle.meshOffset += Vector3.left;
    }
}
