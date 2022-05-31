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
    readonly EcsPool<VehicleComponent> poolVehicle = default;

    readonly MeshGenerator meshGenerator = Service<MeshGenerator>.Get();

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

            var mesh = meshGenerator.CreateVehicleMesh(ref vehicle);
            var mat = Object.FindObjectOfType<WorldOfGodcraft>().mat;

            var root = new GameObject($"Vehicle");
            var view = new GameObject($"Body");
            var renderer = view.AddComponent<MeshRenderer>();
            var meshFilter = view.AddComponent<MeshFilter>();
            var layer = LayerMask.NameToLayer("Vehicle");
            var child = new GameObject("Collider")
            {
                layer = layer
            };
            root.transform.position = startPos;
            view.transform.parent = root.transform;
            view.AddComponent<View>().EntityID = e;
            var collider = child.AddComponent<BoxCollider>();
            collider.center += new Vector3(-0.5f, 0.5f, 0.5f);
            renderer.material = mat;
            meshFilter.mesh = mesh;
            view.transform.position = startPos;
            view.layer = layer;
            child.transform.parent = view.transform;
            child.transform.localPosition = Vector3.zero;

            vehicle.renderer = renderer;
            vehicle.meshFilter = meshFilter;
            vehicle.pos = view.transform.position;
            vehicle.colliders = new() { { Vector3.zero, child } };

            view.AddComponent<Rigidbody>();
        }
    }
}
