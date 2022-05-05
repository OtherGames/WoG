using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System.Linq;
using System.IO;
using LeopotamGroup.Globals;

sealed class PlantsGeneratorSystem : IEcsRunSystem
{
    [EcsFilter(typeof(ChunkInited))]
    readonly EcsFilter chunks = default;
    [EcsWorld]
    readonly EcsWorld world = default;

    MeshGenerator generator = Service<MeshGenerator>.Get();

    public void Run(EcsSystems systems)
    {
        foreach (var entity in chunks)
        {
            var blocks = LoadBlocks();

            int minX = blocks.Min(b => b.pos.x);
            int maxX = blocks.Max(b => b.pos.x);
            int xSize = maxX - minX;

            int minY = blocks.Min(b => b.pos.y);
            int maxY = blocks.Max(b => b.pos.y);
            int ySize = maxY - minY;

            int minZ = blocks.Min(b => b.pos.z);
            int maxZ = blocks.Max(b => b.pos.z);
            int ZSize = maxZ - minZ;

            byte[,,] topology = new byte[xSize + 1, ySize + 1, ZSize + 1];

            foreach (var b in blocks)
            {
                int iX = b.pos.x - minX;
                int iY = b.pos.y - minY;
                int iZ = b.pos.z - minZ;

                topology[iX, iY, iZ] = b.id;
            }

            ref var chunck = ref world.GetPool<ChunkInited>().Get(entity);

            var countByChunck = Random.Range(0, 18);

            List<Vector3Int> positions = new();

            for (int count = 0; count < countByChunck; count++)
            {
                var size = WorldGeneratorInit.size;
                var xRandomIdx = Random.Range(xSize, size - xSize);
                var zRandomIdx = Random.Range(ZSize, size - ZSize);

                var yIdx = 0;

                for (int i = 0; i < size; i++)
                {
                    var yBlock = chunck.chunck.blocks[xRandomIdx, i, zRandomIdx];

                    if (yBlock > 0)
                    {
                        yIdx = i;
                    }
                }

                if (yIdx + ySize + 1 >= size)
                {
                    continue;
                }

                positions.Add(new(xRandomIdx, yIdx, zRandomIdx));
            }

            for (int i = 0; i < positions.Count; i++)
            { 
                for (int x = 0; x < xSize + 1; x++)
                {
                    for (int y = 0; y < ySize + 1; y++)
                    {
                        for (int z = 0; z < ZSize + 1; z++)
                        {
                            var xPos = positions[i].x + x;
                            var yPos = positions[i].y + y + 1;
                            var zPos = positions[i].z + z;

                            chunck.chunck.blocks[xPos, yPos, zPos] = topology[x, y, z];

                            if (y > 0)
                                continue;

                            if (chunck.chunck.blocks[xPos, yPos, zPos] > 0 && chunck.chunck.blocks[xPos, yPos - 1, zPos] == 0)
                            {
                                chunck.chunck.blocks[xPos, yPos - 1, zPos] = 8;
                            }
                        }
                    }
                }
            }

            var otherMesh = generator.UpdateMesh(ref chunck.chunck);
            chunck.chunck.meshFilter.mesh = otherMesh;
            chunck.chunck.collider.sharedMesh = otherMesh;
        }
    }

    //private Vector3Int GetRandomUpperPos()
    //{

    //}

    List<WritableBlock> LoadBlocks()
    {
        var path = Application.dataPath + "/Data/tree.json";
        if (File.Exists(path))
        {
            var file = File.ReadAllText(path);
            
            return Json.Deserialize<List<WritableBlock>>(file);
        }

        return null;
    }
}
