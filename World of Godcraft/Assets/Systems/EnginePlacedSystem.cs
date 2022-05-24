using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite;
using UnityEngine;

sealed class EnginePlacedSystem : IEcsRunSystem
{
    [EcsFilter(typeof(EnginePlaced))]
    readonly EcsFilter filterPlaced = default;
    [EcsPool]
    readonly EcsPool<EnginePlaced> poolPlaced = default;
    [EcsPool]
    EcsPool<VehicleComponent> poolVehicle = default;

    MeshGenerator meshGenerator = Service<MeshGenerator>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filterPlaced)
        {
            var startPos = poolPlaced.Get(entity).pos;

            var e = systems.GetWorld().NewEntity();
            ref var vehicle = ref poolVehicle.Add(e);
            vehicle.size = 1;
            vehicle.blocks = new();
            vehicle.blocks.Add(new());
            for (int x = 0; x < vehicle.size; x++)
            {
                vehicle.blocks[x] = new();
                vehicle.blocks[x].Add(new());

                for (int y = 0; y < vehicle.size; y++)
                {
                    vehicle.blocks[x][y] = new();
                    vehicle.blocks[x][y].Add(new());

                    for (int z = 0; z < vehicle.size; z++)
                    {
                        vehicle.blocks[x][y][z] = BLOCKS.ENGINE;
                    }
                }
            }

            var mesh = meshGenerator.CreateVehicleMesh(ref vehicle, Vector3Int.zero);
            var mat = Object.FindObjectOfType<WorldOfGodcraft>().mat;

            var view = new GameObject($"Vehicle");
            var renderer = view.AddComponent<MeshRenderer>();
            var meshFilter = view.AddComponent<MeshFilter>();
            var collider = view.AddComponent<MeshCollider>();
            collider.convex = true;
            renderer.material = mat;
            meshFilter.mesh = mesh;
            collider.sharedMesh = mesh;
            view.transform.position = startPos;
            view.layer = LayerMask.NameToLayer($"Vehicle");

            vehicle.renderer = renderer;
            vehicle.meshFilter = meshFilter;
            vehicle.collider = collider;
            vehicle.pos = view.transform.position;

            view.AddComponent<Rigidbody>();
        }
    }
}
