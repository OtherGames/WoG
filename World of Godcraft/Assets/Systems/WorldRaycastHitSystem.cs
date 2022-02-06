using Leopotam.EcsLite;
using LeopotamGroup.Globals;
using Leopotam.EcsLite.Di;
using UnityEngine;
using static WorldGeneratorInit;

sealed class WorldRaycastHitSystem : IEcsRunSystem
{
    [EcsFilter(typeof(ChunckHitEvent))]
    // field will be injected with filter (ChunckHitEvent included)
    // from default world instance.
    readonly EcsFilter hitFilter = default;

    [EcsInject]
    readonly World chunckWorld = default;

    public void Run(EcsSystems systems)
    {
        var world = systems.GetWorld();

        var chunckFilter = world.Filter<ChunckComponent>().End();

        foreach (var hitEntity in hitFilter)
        {
            ref var hitComponent = ref world.GetPool<ChunckHitEvent>().Get(hitEntity);

            ref var chunck = ref chunckWorld.GetChunk(hitComponent.position);

            WorldHit(ref chunck, ref hitComponent);

            //foreach (var chunckEntity in chunckFilter)
            //{
            //    ref var chunckComponent = ref world.GetPool<ChunckComponent>().Get(chunckEntity);

            //    if(chunckComponent.collider == hitComponent.collider)
            //    {
            //        WorldHit(ref chunckComponent, ref hitComponent);
            //    }
            //}
        }
    }

    void WorldHit(ref ChunckComponent chunck, ref ChunckHitEvent hit)
    {
        var pos = chunck.renderer.transform.position;

        int x = (int)(hit.position.x - pos.x) + 1;
        int y = (int)(hit.position.y - pos.y);
        int z = (int)(hit.position.z - pos.z);

        chunck.blocks[x, y, z] = hit.blockId;

        var generator = Service<MeshGenerator>.Get();
        var mesh = generator.UpdateMesh(ref chunck);
        chunck.meshFilter.mesh = mesh;
        chunck.collider.sharedMesh = mesh;

        for (int p = 0; p < 6; p++)
        {
            var blockPos = new Vector3(x, y, z);

            Vector3 checkingBlockPos = blockPos + World.faceChecks[p];
            var blockInOtherChunckPos = checkingBlockPos + pos;

            int xIdx = Mathf.FloorToInt(blockInOtherChunckPos.x / size);
            int zIdx = Mathf.FloorToInt(blockInOtherChunckPos.z / size);

            if (xIdx < 0 || xIdx >= worldSize || zIdx < 0 || zIdx >= worldSize)
                continue;

            if (!IsBlockChunk((int)checkingBlockPos.x, (int)checkingBlockPos.y, (int)checkingBlockPos.z))
            {
                ref var otherChunck = ref chunckWorld.GetChunk(checkingBlockPos + pos);
              
                var otherMesh = generator.UpdateMesh(ref otherChunck);
                otherChunck.meshFilter.mesh = otherMesh;
                otherChunck.collider.sharedMesh = otherMesh;  
            }

        }

    }

    bool IsBlockChunk(int x, int y, int z)
    {
        if (x < 0 || x > size - 1 || y < 0 || y > size - 1 || z < 0 || z > size - 1)
            return false;
        else
            return true;
    }

}
