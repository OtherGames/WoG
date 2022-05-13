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

    [EcsWorld]
    readonly EcsWorld world = default;

    float hitTimer = 0;
    int countHit = 0;

    public void Run(EcsSystems systems)
    {
        var world = systems.GetWorld();

        var chunckFilter = world.Filter<ChunckComponent>().End();

        if (hitFilter.GetEntitiesCount() == 0)
        {
            hitTimer = 0;
            countHit = 0;
            return;
        }

        hitTimer += Time.deltaTime;

        foreach (var hitEntity in hitFilter)
        {
            var pool = world.GetPool<ChunckHitEvent>();

            ref var hitComponent = ref pool.Get(hitEntity);

            if(hitComponent.blockId > 0)
            {
                countHit = 888;
            }

            if (hitTimer > 0.01f)
            {
                countHit++;
                hitTimer = 0;

                Debug.Log(countHit);
            }

            if (countHit > 8)
            {
                // зачем-то нужно прибавл€ть 1 по оси X, хз почему так, но именно так работает
                var fixedPos = hitComponent.position + Vector3.right;

                ref var chunck = ref chunckWorld.GetChunk(fixedPos);

                WorldHit(ref chunck, ref hitComponent);

                pool.Del(hitEntity);

            }
        }
    }

    void WorldHit(ref ChunckComponent chunck, ref ChunckHitEvent hit)
    {
        var pos = chunck.renderer.transform.position;

        // зачем-то нужно прибавл€ть 1 по оси X, хз почему так, но именно так работает
        int x = (int)(hit.position.x - pos.x) + 1;
        int y = (int)(hit.position.y - pos.y);
        int z = (int)(hit.position.z - pos.z);

        //chunck = ref chunckWorld.GetChunk(new Vector3(x, y, z));

        byte blockID = chunck.blocks[x, y, z];
        chunck.blocks[x, y, z] = hit.blockId;

        var generator = Service<MeshGenerator>.Get();
        var mesh = generator.UpdateMesh(ref chunck);//, (int)pos.x, (int)pos.y, (int)pos.z);
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

        if (hit.blockId == 0)
            CreateDropedBlock(blockID, x + pos.x, y + pos.y, z + pos.z);
    }

    bool IsBlockChunk(int x, int y, int z)
    {
        if (x < 0 || x > size - 1 || y < 0 || y > size - 1 || z < 0 || z > size - 1)
            return false;
        else
            return true;
    }

    void CreateDropedBlock(byte id, float x, float y, float z)
    {
        if (id == 2)
        {
            id = 3;
        }

        var dropedMeshGenerator = Service<DropedBlockGenerator>.Get();
        var dropedBlock = new GameObject("Droped Block " + x + y + z);
        dropedBlock.AddComponent<MeshRenderer>().material = Object.FindObjectOfType<WorldOfGodcraft>().mat;
        dropedBlock.AddComponent<MeshFilter>().mesh = dropedMeshGenerator.GenerateMesh(id);
        dropedBlock.AddComponent<DropedBlock>();
        dropedBlock.transform.localScale /= 3f;

        float offsetRandomX = Random.Range(0.3f, 0.57f);
        float offsetRandomY = Random.Range(0.3f, 0.57f);
        float offsetRandomZ = Random.Range(0.3f, 0.57f);

        dropedBlock.transform.position = new(x - offsetRandomX, y + offsetRandomY, z + offsetRandomZ);

        var entity = world.NewEntity();
        var pool = world.GetPool<DropedComponent>();
        pool.Add(entity);
        ref var component = ref pool.Get(entity);
        component.BlockID = id;
        component.view = dropedBlock;
    }

}
