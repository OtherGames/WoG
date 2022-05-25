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

    MeshGenerator meshGenerator = Service<MeshGenerator>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterHitEvent)
        {
            ref var hitEvent = ref poolHitEvent.Get(entity);
            hitEvent.blockPos += Vector3Int.back;

            foreach (var entityVehicle in filterVehicle)
            {
                ref var vehicle = ref poolVehicle.Get(entityVehicle);
                var pos = hitEvent.blockPos;
                if(pos.x >= vehicle.size || pos.y >= vehicle.size || pos.z >= vehicle.size || pos.x < 0 || pos.y < 0 || pos.z < 0)
                {
                    vehicle.size++;
                    vehicle.blocks.Add(new());

                    for (int i = 0; i < vehicle.size; i++)
                    {
                        while(vehicle.blocks[i].Count < vehicle.size)
                        {
                            vehicle.blocks[i].Add(new());
                        }
                    }

                    for (int i = 0; i < vehicle.size; i++)
                    {
                        for (int j = 0; j < vehicle.size; j++)
                        {
                            while(vehicle.blocks[i][j].Count < vehicle.size)
                            {
                                vehicle.blocks[i][j].Add(2);
                            }
                        }
                    }

                    List<List<List<byte>>> blocksCopy = new();
                    blocksCopy.AddRange(vehicle.blocks);

                    for (int i = vehicle.size - 1; i >= 0; i--)
                    {
                        for (int j = vehicle.size - 1; j >= 0; j--)
                        {
                            for (int z = vehicle.size - 1; z >= 0; z--)
                            {
                                Debug.Log(blocksCopy[i][j][z]);
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

                var mesh = meshGenerator.UpdateVehicleMesh(ref vehicle);
                vehicle.meshFilter.mesh = mesh;
                vehicle.collider.sharedMesh = mesh;
            }
        }
    }
}
