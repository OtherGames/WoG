using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using LeopotamGroup.Globals;
using System.Collections.Generic;
using UnityEngine;

sealed class GenerateMorkvaSystem : IEcsRunSystem
{
    [EcsFilter(typeof(ChunkInited))]
    readonly EcsFilter filter = default;
    [EcsWorld]
    readonly EcsWorld world = default;
    readonly PrefabsHolder prefabs = Service<PrefabsHolder>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in filter)
        {
            ref var component = ref world.GetPool<ChunkInited>().Get(entity);

            var positions = GeneratePositions(ref component);

            foreach (var pos in positions)
            {
                var morkva = Object.Instantiate(prefabs.morkva, pos, Quaternion.identity);
                morkva.transform.parent = component.chunck.collider.transform;

                var e = systems.GetWorld().NewEntity();
                ref var pickable = ref systems.GetWorld().GetPool<PickableComponent>().Add(e);
                pickable.view = morkva;
                pickable.name = "Морква";
                pickable.id = ITEMS.MORKVA;

                ref var food = ref systems.GetWorld().GetPool<FoodComponent>().Add(e);
                food.view = morkva;
                food.id = 170;
                food.satietyValue = 10;
                food.freshMax = 300;
                food.freshValue = food.freshMax;
            }
        }
    }


    List<Vector3> GeneratePositions(ref ChunkInited component)
    {
        var countByChunck = Random.Range(0, 8);

        List<Vector3> positions = new();

        for (int count = 0; count < countByChunck; count++)
        {
            var size = WorldGeneratorInit.size;
            var xRandomIdx = Random.Range(0, size);
            var zRandomIdx = Random.Range(0, size);

            var yIdx = 0;

            for (int i = 0; i < size; i++)
            {
                var yBlock = component.chunck.blocks[xRandomIdx, i, zRandomIdx];

                if (yBlock > 0)
                {
                    yIdx = i;
                }
            }

            if (yIdx + 1 >= size || component.chunck.blocks[xRandomIdx, yIdx, zRandomIdx] != 1)
            {
                continue;
            }

            Debug.Log(component.chunck.pos);
            var x = component.chunck.pos.x + xRandomIdx - Random.Range(0.1f, 0.9f);
            var z = component.chunck.pos.z + zRandomIdx + Random.Range(0.1f, 0.9f);
            var y = yIdx + Random.Range(0.73f, 0.9f);
            positions.Add(new(x, y, z));
        }

        return positions;
    }
}
