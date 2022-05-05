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
                    chunckComponent.blocks[x, y, z] = BlockExist(x + posX, y + posY, z + posZ);                    
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
                        BlockUVS b = new(0, 15, 3, 15, 2, 15);

                        if (x == 0 && z == 0)
                            b = new BlockUVS(2, 15);
                        //BlockUVS b = new BlockUVS(1, 15);
                        //BlockUVS b = new(2, 15);

                        if ((z + 1 >= size && BlockExist(x + posX, y + posY, z + 1 + posZ) == 0) || (!(z + 1 >= size) && chunck.blocks[x, y, z + 1] == 0))
                        {
                            CreateBlockSide(BlockSide.Front, x, y, z, b);
                        }
                        if ((z - 1 < 0 && BlockExist(x + posX, y + posY, z - 1 + posZ) == 0) || (!(z - 1 < 0) && chunck.blocks[x, y, z - 1] == 0))
                        {
                            CreateBlockSide(BlockSide.Back, x, y, z, b);
                        }
                        if ((x + 1 >= size && BlockExist(x + 1 + posX, y + posY, z + posZ) == 0) || (!(x + 1 >= size) && chunck.blocks[x + 1, y, z] == 0))
                        {
                            CreateBlockSide(BlockSide.Right, x, y, z, b);
                        }
                        if ((x - 1 < 0 && BlockExist(x - 1 + posX, y + posY, z + posZ) == 0) || (!(x - 1 < 0) && chunck.blocks[x - 1, y, z] == 0))
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

    public byte BlockExist(int x, int y, int z)
    {
        Random.InitState(505);

        int k = 10000;

        Vector3 offset = new(Random.value * k, Random.value * k, Random.value * k);
        //offset = Vector3.zero;
        float noiseX = Mathf.Abs((float)(x + offset.x) / noiseScale);
        float noiseY = Mathf.Abs((float)(y + offset.y) / noiseScale);
        float noiseZ = Mathf.Abs((float)(z + offset.z) / noiseScale);

        float noiseValue = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);
        //float cavernas = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);

        noiseValue += (30 - y) / 30f;// World bump
        noiseValue /= y / 8f;

        //cavernas /= y / 19f;
        //cavernas /= 2;

        if (noiseValue > 0.2f)
        {
            return 1;
        }

        return 0;

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
}


public class BlockUVS
{
    public int TextureX;
    public int TextureY;

    public int TextureXSide;
    public int TextureYSide;

    public int TextureXBottom;
    public int TextureYBottom;

    public BlockUVS(int tX, int tY, int sX, int sY, int bX, int bY)
    {
        TextureX = tX;
        TextureY = tY;
        TextureXSide = sX;
        TextureYSide = sY;
        TextureXBottom = bX;
        TextureYBottom = bY;
    }

    public BlockUVS(int tX, int tY, int sX, int sY)
    {
        TextureX = tX;
        TextureY = tY;
        TextureXSide = sX;
        TextureYSide = sY;
        TextureXBottom = tX;
        TextureYBottom = tY;
    }

    public BlockUVS(int tX, int tY)
    {
        TextureX = tX;
        TextureY = tY;
        TextureXSide = tX;
        TextureYSide = tY;
        TextureXBottom = tX;
        TextureYBottom = tY;
    }

    public static BlockUVS GetBlock (byte id)
    {
        switch (id)
        {
            case 1:
                return new BlockUVS(0, 15, 3, 15, 2, 15);
            case 2:
                return new BlockUVS(1, 15);
            case 3:
                return new BlockUVS(2, 15);
            case 4:
                return new BlockUVS(0, 14);
            case 5:
                return new BlockUVS(1, 1);
            case 6:
                return new BlockUVS(1, 3);
            case 7:
                return new BlockUVS(4, 2);
            case 8:
                return new BlockUVS(4, 14);
            case 9:
                return new BlockUVS(0, 2);
            case 10:
                return new BlockUVS(5, 12);

        }

        return new BlockUVS(0, 15, 3, 15, 2, 15);
    }
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

