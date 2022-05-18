using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System.Collections.Generic;
using UnityEngine;


sealed class WorldGeneratorInit : IEcsInitSystem
{
    [EcsInject]
    readonly World chuncksWorld;
    [EcsWorld]
    readonly EcsWorld ecsWorld = default;


    public const int worldSize = 5;
    public const int size = 32;
    public const int noiseScale = 100;
    public const float TextureOffset = 1f / 16f;
    public const float landThresold = 0.11f;
    public const float smallRockThresold = 0.8f;

    Dictionary<BlockSide, List<Vector3>> blockVerticesSet;
    Dictionary<BlockSide, List<int>> blockTrianglesSet;

    readonly List<Vector3> vertices = new();
    readonly List<int> triangulos = new();
    readonly List<Vector2> uvs = new();


    public void Init(EcsSystems systems)
    {
        blockVerticesSet = new Dictionary<BlockSide, List<Vector3>>();
        blockTrianglesSet = new Dictionary<BlockSide, List<int>>();

        DictionaryInits();
        InitTriangulos();

        for (int x = 0; x < size * worldSize; x += size)
        {
            for (int z = 0; z < size * worldSize; z += size)
            {
                int xIdx = Mathf.FloorToInt((float)x / size);
                int zIdx = Mathf.FloorToInt((float)z / size);

                chuncksWorld.chuncks[xIdx, 0, zIdx] = CreateChunck(systems, x, 0, z);
            }
        }

        Debug.Log(minValue + " --- " + maxValue);
    }

    ChunckComponent CreateChunck(EcsSystems systems, int posX, int posY, int posZ)
    {
        vertices?.Clear();
        triangulos?.Clear();
        uvs?.Clear();

        var world = systems.GetWorld();
        int chunckEntity = world.NewEntity();
        var chuncksPool = world.GetPool<ChunckComponent>();
        chuncksPool.Add(chunckEntity);

        ref var chunckComponent = ref chuncksPool.Get(chunckEntity);
        chunckComponent.blocks = new byte[size, size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    chunckComponent.blocks[x, y, z] = GeneratedBlockID(x + posX, y + posY, z + posZ);                    
                }
            }
        }


        var mat = Object.FindObjectOfType<WorldOfGodcraft>().mat;
        var mesh = GenerateMesh(ref chunckComponent, posX, posY, posZ);

        var chunck = new GameObject($"Chunck {posX} {posY} {posZ}");
        var renderer = chunck.AddComponent<MeshRenderer>();
        var meshFilter = chunck.AddComponent<MeshFilter>();
        var collider = chunck.AddComponent<MeshCollider>();
        renderer.material = mat;
        meshFilter.mesh = mesh;
        collider.sharedMesh = mesh;
        chunck.transform.position = new Vector3(posX, posY, posZ);

        chunckComponent.renderer = renderer;
        chunckComponent.meshFilter = meshFilter;
        chunckComponent.collider = collider;
        chunckComponent.pos = chunck.transform.position;

        RaiseChunkInitEvent(chunckComponent);

        return chunckComponent;
        
    }

    void RaiseChunkInitEvent(ChunckComponent chunck)
    {
        var e = ecsWorld.NewEntity();
        var pool = ecsWorld.GetPool<ChunkInited>();
        pool.Add(e);
        ref var component = ref pool.Get(e);
        component.chunck = chunck;
    }

    Mesh GenerateMesh(ref ChunckComponent chunck, int posX, int posY, int posZ)
    {
        Mesh mesh = new();
        mesh.Clear();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (chunck.blocks[x, y, z] > 0)
                    {
                        BlockUVS b = BlockUVS.GetBlock(chunck.blocks[x, y, z]);

                        if (x == 0 && z == 0)
                            b = new BlockUVS(2, 15);

                        if ((z + 1 >= size && GeneratedBlockID(x + posX, y + posY, z + 1 + posZ) == 0) || (!(z + 1 >= size) && chunck.blocks[x, y, z + 1] == 0))
                        {
                            CreateBlockSide(BlockSide.Front, x, y, z, b);
                        }
                        if ((z - 1 < 0 && GeneratedBlockID(x + posX, y + posY, z - 1 + posZ) == 0) || (!(z - 1 < 0) && chunck.blocks[x, y, z - 1] == 0))
                        {
                            CreateBlockSide(BlockSide.Back, x, y, z, b);
                        }
                        if ((x + 1 >= size && GeneratedBlockID(x + 1 + posX, y + posY, z + posZ) == 0) || (!(x + 1 >= size) && chunck.blocks[x + 1, y, z] == 0))
                        {
                            CreateBlockSide(BlockSide.Right, x, y, z, b);
                        }
                        if ((x - 1 < 0 && GeneratedBlockID(x - 1 + posX, y + posY, z + posZ) == 0) || (!(x - 1 < 0) && chunck.blocks[x - 1, y, z] == 0))
                        {
                            CreateBlockSide(BlockSide.Left, x, y, z, b);
                        }
                        if (!(y + 1 >= size) && chunck.blocks[x, y + 1, z] == 0 || y + 1 >= size)
                        {
                            CreateBlockSide(BlockSide.Top, x, y, z, b);
                        }
                        if (!(y - 1 < 0) && chunck.blocks[x, y - 1, z] == 0)
                        {
                            CreateBlockSide(BlockSide.Bottom, x, y, z, b);
                        }
                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangulos.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.OptimizeReorderVertexBuffer();
        mesh.Optimize();

        return mesh;
    }

    void DictionaryInits()
    {
        List<Vector3> verticesFront = new List<Vector3>
            {
                new Vector3( 0, 0, 1 ),
                new Vector3(-1, 0, 1 ),
                new Vector3(-1, 1, 1 ),
                new Vector3( 0, 1, 1 ),
            };
        List<Vector3> verticesBack = new List<Vector3>
            {
                new Vector3( 0, 0, 0 ),
                new Vector3(-1, 0, 0 ),
                new Vector3(-1, 1, 0 ),
                new Vector3( 0, 1, 0 ),
            };
        List<Vector3> verticesRight = new List<Vector3>
            {
                new Vector3( 0, 0, 0 ),
                new Vector3( 0, 0, 1 ),
                new Vector3( 0, 1, 1 ),
                new Vector3( 0, 1, 0 ),
            };
        List<Vector3> verticesLeft = new List<Vector3>
            {
                new Vector3(-1, 0, 0 ),
                new Vector3(-1, 0, 1 ),
                new Vector3(-1, 1, 1 ),
                new Vector3(-1, 1, 0 ),
            };
        List<Vector3> verticesTop = new List<Vector3>
            {
                new Vector3( 0, 1, 0 ),
                new Vector3(-1, 1, 0 ),
                new Vector3(-1, 1, 1 ),
                new Vector3( 0, 1, 1 ),
            };
        List<Vector3> verticesBottom = new List<Vector3>
            {
                new Vector3( 0, 0, 0 ),
                new Vector3(-1, 0, 0 ),
                new Vector3(-1, 0, 1 ),
                new Vector3( 0, 0, 1 ),
            };

        blockVerticesSet.Add(BlockSide.Front, null);
        blockVerticesSet.Add(BlockSide.Back, null);
        blockVerticesSet.Add(BlockSide.Right, null);
        blockVerticesSet.Add(BlockSide.Left, null);
        blockVerticesSet.Add(BlockSide.Top, null);
        blockVerticesSet.Add(BlockSide.Bottom, null);

        blockVerticesSet[BlockSide.Front] = verticesFront;//.ToNativeArray(Allocator.Persistent);
        blockVerticesSet[BlockSide.Back] = verticesBack;//.ToNativeArray(Allocator.Persistent);
        blockVerticesSet[BlockSide.Right] = verticesRight;//.ToNativeArray(Allocator.Persistent);
        blockVerticesSet[BlockSide.Left] = verticesLeft;//.ToNativeArray(Allocator.Persistent);
        blockVerticesSet[BlockSide.Top] = verticesTop;//.ToNativeArray(Allocator.Persistent);
        blockVerticesSet[BlockSide.Bottom] = verticesBottom;
    }

    void InitTriangulos()
    {
        List<int> trianglesFront = new List<int> { 3, 2, 1, 4, 3, 1 };
        List<int> trianglesBack = new List<int> { 1, 2, 3, 1, 3, 4 };
        List<int> trianglesRight = new List<int> { 1, 3, 2, 4, 3, 1 };
        List<int> trianglesLeft = new List<int> { 2, 3, 1, 1, 3, 4 };
        List<int> trianglesTop = new List<int> { 1, 2, 3, 1, 3, 4 };
        List<int> trianglesBottom = new List<int> { 3, 2, 1, 4, 3, 1 };

        blockTrianglesSet.Add(BlockSide.Front, trianglesFront);
        blockTrianglesSet.Add(BlockSide.Back, trianglesBack);
        blockTrianglesSet.Add(BlockSide.Right, trianglesRight);
        blockTrianglesSet.Add(BlockSide.Left, trianglesLeft);
        blockTrianglesSet.Add(BlockSide.Top, trianglesTop);
        blockTrianglesSet.Add(BlockSide.Bottom, trianglesBottom);
    }


    void CreateBlockSide(BlockSide side, int x, int y, int z, BlockUVS b)
    {
        List<Vector3> vrtx = blockVerticesSet[side];
        List<int> trngls = blockTrianglesSet[side];
        int offset = 1;

        triangulos.Add(trngls[0] - offset + vertices.Count);
        triangulos.Add(trngls[1] - offset + vertices.Count);
        triangulos.Add(trngls[2] - offset + vertices.Count);

        triangulos.Add(trngls[3] - offset + vertices.Count);
        triangulos.Add(trngls[4] - offset + vertices.Count);
        triangulos.Add(trngls[5] - offset + vertices.Count);

        vertices.Add(new Vector3(x + vrtx[0].x, y + vrtx[0].y, z + vrtx[0].z)); // 1
        vertices.Add(new Vector3(x + vrtx[1].x, y + vrtx[1].y, z + vrtx[1].z)); // 2
        vertices.Add(new Vector3(x + vrtx[2].x, y + vrtx[2].y, z + vrtx[2].z)); // 3
        vertices.Add(new Vector3(x + vrtx[3].x, y + vrtx[3].y, z + vrtx[3].z)); // 4

        AddUVS(side, b);
    }

    void AddUVS(BlockSide side, BlockUVS b)
    {
        switch (side)
        {
            case BlockSide.Front:
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, (TextureOffset * b.TextureYSide) + TextureOffset));
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, (TextureOffset * b.TextureYSide) + TextureOffset));
                break;
            case BlockSide.Back:
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, (TextureOffset * b.TextureYSide) + TextureOffset));
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, (TextureOffset * b.TextureYSide) + TextureOffset));
                break;
            case BlockSide.Right:
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, (TextureOffset * b.TextureYSide) + TextureOffset));
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, (TextureOffset * b.TextureYSide) + TextureOffset));

                break;
            case BlockSide.Left:
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, TextureOffset * b.TextureYSide));
                uvs.Add(new Vector2((TextureOffset * b.TextureXSide) + TextureOffset, (TextureOffset * b.TextureYSide) + TextureOffset));
                uvs.Add(new Vector2(TextureOffset * b.TextureXSide, (TextureOffset * b.TextureYSide) + TextureOffset));

                break;
            case BlockSide.Top:
                uvs.Add(new Vector2(TextureOffset * b.TextureX, TextureOffset * b.TextureY));
                uvs.Add(new Vector2((TextureOffset * b.TextureX) + TextureOffset, TextureOffset * b.TextureY));
                uvs.Add(new Vector2((TextureOffset * b.TextureX) + TextureOffset, (TextureOffset * b.TextureY) + TextureOffset));
                uvs.Add(new Vector2(TextureOffset * b.TextureX, (TextureOffset * b.TextureY) + TextureOffset));

                break;
            case BlockSide.Bottom:
                uvs.Add(new Vector2(TextureOffset * b.TextureXBottom, TextureOffset * b.TextureYBottom));
                uvs.Add(new Vector2((TextureOffset * b.TextureXBottom) + TextureOffset, TextureOffset * b.TextureYBottom));
                uvs.Add(new Vector2((TextureOffset * b.TextureXBottom) + TextureOffset, (TextureOffset * b.TextureYBottom) + TextureOffset));
                uvs.Add(new Vector2(TextureOffset * b.TextureXBottom, (TextureOffset * b.TextureYBottom) + TextureOffset));

                break;

        }

    }

    public byte GeneratedBlockID(int x, int y, int z)
    {
        Random.InitState(505);

        // ============== Генерация Гор =============
        var k = 10000000;// чем больше тем реже

        Vector3 offset = new(Random.value * k, Random.value * k, Random.value * k);

        float noiseX = Mathf.Abs((float)(x + offset.x) / noiseScale / 2);
        float noiseY = Mathf.Abs((float)(y + offset.y) / noiseScale / 2);
        float noiseZ = Mathf.Abs((float)(z + offset.z) / noiseScale / 2);

        float goraValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        goraValue += (30 - y) / 3000f;// World bump
        //goraValue /= y / 1f;// для воды заебок;

        byte blockID = 0;
        if (goraValue > 0.35f)
        {
            if (goraValue > 0.3517f)
            {
                blockID = 2;
            }
            else
            {
                blockID = 1;
            }
        }
        // ==========================================

        // =========== Основной ландшафт ============
        k = 10000;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / noiseScale);
        noiseY = Mathf.Abs((float)(y + offset.y) / noiseScale);
        noiseZ = Mathf.Abs((float)(z + offset.z) / noiseScale);

        float noiseValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        noiseValue += (30 - y) / 30f;// World bump
        noiseValue /= y / 8f;

        //cavernas /= y / 19f;
        //cavernas /= 2;
        //Debug.Log($"{noiseValue} --- {y}");

        if (noiseValue > landThresold)
        {
            if (noiseValue > 0.5f)
            {
                blockID = 2;
            }
            else
            {
                blockID = 1;
            }
        }
        // ==========================================

        // =========== Скалы, типа пики =============
        k = 10000;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / (noiseScale * 2));
        noiseY = Mathf.Abs((float)(y + offset.y) / (noiseScale * 3));
        noiseZ = Mathf.Abs((float)(z + offset.z) / (noiseScale * 2));

        float rockValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        if (rockValue > 0.8f)
        {
            if (rockValue > 0.801f)
                blockID = 2;
            else
                blockID = 1;
        }
        // ==========================================

        // =========== Скалы, типа пики =============
        k = 100;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / (noiseScale / 2));
        noiseY = Mathf.Abs((float)(y + offset.y) / (noiseScale / 1));
        noiseZ = Mathf.Abs((float)(z + offset.z) / (noiseScale / 2));

        float smallRockValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        if (smallRockValue > smallRockThresold && noiseValue > (landThresold - 0.08f))
        {
            blockID = 2;
            if (smallRockValue < smallRockThresold + 0.01f)
                blockID = 1;
        }
        // ==========================================


        // =========== Уголь ========================
        k = 10;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / (noiseScale / 9));
        noiseY = Mathf.Abs((float)(y + offset.y) / (noiseScale / 9));
        noiseZ = Mathf.Abs((float)(z + offset.z) / (noiseScale / 9));

        float coalValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        if (coalValue > 0.92f && (noiseValue > landThresold))
        {
            blockID = BLOCKS.ORE_COAL;
        }
        // ==========================================

        // =========== Жэлэзная руда ========================
        k = 700;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / (noiseScale / 9));
        noiseY = Mathf.Abs((float)(y + offset.y) / (noiseScale / 9));
        noiseZ = Mathf.Abs((float)(z + offset.z) / (noiseScale / 9));

        float oreValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        if (oreValue > 0.93f && (noiseValue > landThresold))
        {
            blockID = 30;
        }
        // ==========================================

        // =========== Селитра руда ========================
        k = 635;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / (noiseScale / 9));
        noiseY = Mathf.Abs((float)(y + offset.y) / (noiseScale / 9));
        noiseZ = Mathf.Abs((float)(z + offset.z) / (noiseScale / 9));

        float saltpeterValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        if (saltpeterValue > 0.935f && (noiseValue > landThresold))
        {
            blockID = BLOCKS.SALTPETER;
        }
        // ==========================================

        // =========== Сера ========================
        k = 364789;

        offset = new(Random.value * k, Random.value * k, Random.value * k);

        noiseX = Mathf.Abs((float)(x + offset.x) / (noiseScale / 9));
        noiseY = Mathf.Abs((float)(y + offset.y) / (noiseScale / 9));
        noiseZ = Mathf.Abs((float)(z + offset.z) / (noiseScale / 9));

        float sulfurValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        if (sulfurValue > 0.93f && (noiseValue > landThresold))
        {
            blockID = BLOCKS.ORE_SULFUR;
        }
        // ==========================================


        // Типа горы
        ////////// Для рек ////////////////////////////////////////////
        //k = 10000000;// чем больше тем реже

        //offset = new(Random.value * k, Random.value * k, Random.value * k);

        //noiseX = Mathf.Abs((float)(x + offset.x) / noiseScale / 2);
        //noiseY = Mathf.Abs((float)(y + offset.y) / noiseScale / 2);
        //noiseZ = Mathf.Abs((float)(z + offset.z) / noiseScale / 2);

        //float goraValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        //goraValue += (30 - y) / 3000f;// World bump
        //goraValue /= y / 80f;// для воды заебок;

        //blockID = 0;
        //if (goraValue > 0.08f && goraValue < 0.3f)
        //{
        //    blockID = 2;
        //}
        ///==============================================



        if (oreValue < minValue)
            minValue = oreValue;
        if (oreValue > maxValue)
            maxValue = oreValue;

        /////////////////////////////////////////////////////////////////////
        //k = 10000000;// чем больше тем реже

        //offset = new(Random.value * k, Random.value * k, Random.value * k);

        //noiseX = Mathf.Abs((float)(x + offset.x) / noiseScale / 2);
        //noiseY = Mathf.Abs((float)(y + offset.y) / noiseScale * 2);
        //noiseZ = Mathf.Abs((float)(z + offset.z) / noiseScale / 2);

        //float goraValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        //goraValue += (30 - y) / 30000f;// World bump
        //goraValue /= y / 8f;

        ////blockID = 0;
        //if (goraValue > 0.1f && goraValue < 0.3f)
        //{
        //    blockID = 2;
        //}
        ////////////////////////////////////////////////////////////////

        return blockID;

        //Random.InitState(888);
        //int k = 1000000;
        //Vector3 offset = new Vector3(Random.value * k, Random.value * k, Random.value * k);

        //Vector3 pos = new Vector3
        //(
        //    x + offset.x,
        //    y + offset.y,
        //    z + offset.z
        //);
        //float noiseX = Mathf.Abs((float)(pos.x + offset.x) / ebota);
        //float noiseY = Mathf.Abs((float)(pos.y + offset.y) / ebota);
        //float noiseZ = Mathf.Abs((float)(pos.z + offset.z) / ebota);
        //#pragma warning disable CS0436 // Тип конфликтует с импортированным типом
        //            var res = noise.snoise(new float3(noiseX, noiseY, noiseZ));//snoise(pos);
        //#pragma warning restore CS0436 // Тип конфликтует с импортированным типом

        //if (y < 3) res = 0.5f;

        //if (res > 0.3f)
        //{
        //    return true;
        //}


    }
    float minValue = float.MaxValue;
    float maxValue = float.MinValue;
}




public enum BlockSide : byte
{
    Front,
    Back,
    Right,
    Left,
    Top,
    Bottom
}

public enum BlockType : byte
{
    Grass,
    Dirt,
    Stone
}

