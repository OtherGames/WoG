using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeopotamGroup.Globals;
using static WorldGeneratorInit;

public class MeshGenerator 
{
    Dictionary<BlockSide, List<Vector3>> blockVerticesSet;
    Dictionary<BlockSide, List<int>> blockTrianglesSet;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangulos = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public void Init()
    {
        blockVerticesSet = new Dictionary<BlockSide, List<Vector3>>();
        blockTrianglesSet = new Dictionary<BlockSide, List<int>>();

        vertices = new List<Vector3>();
        triangulos = new List<int>();
        uvs = new List<Vector2>();

        DictionaryInits();
        InitTriangulos();
    }

    internal Mesh GenerateMesh(ref ChunckComponent chunck, int posX, int posY, int posZ)
    {
        vertices.Clear();
        triangulos.Clear();
        uvs.Clear();

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
                        //var xOutMax = x + 1 + posX >= size * worldSize;
                        //var xOutMin = x - 1 < 0;
                        //var zOutMax = z + 1 + posZ >= size * worldSize;
                        //var zOutMin = z - 1 < 0;

                        //if (xOutMax || xOutMin || zOutMax || zOutMin)
                        //    continue;


                        BlockUVS b = new(0, 15, 3, 15, 2, 15);
                        if (x == 0 && z == 0)
                            b = new BlockUVS(1, 15);
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

    internal Mesh UpdateMesh(ref ChunckComponent chunck)
    {
        int posX = (int)chunck.renderer.transform.position.x;
        int posY = (int)chunck.renderer.transform.position.y;
        int posZ = (int)chunck.renderer.transform.position.z;

        vertices.Clear();
        triangulos.Clear();
        uvs.Clear();

        Mesh mesh = new();
        mesh.Clear();
        var w = Service<World>.Get();

        for (int x = 0; x < size; x++)
        {
            var xOutMax = x + 1 + posX >= size * worldSize;
            var xOutMin = x - 1 + posX < 0;

            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (chunck.blocks[x, y, z] > 0)
                    {
                        
                        var zOutMax = z + 1 + posZ >= size * worldSize;
                        var zOutMin = z - 1 + posZ < 0;

                        BlockUVS b = BlockUVS.GetBlock(chunck.blocks[x, y, z]); //new(0, 15, 3, 15, 2, 15);

                        //BlockUVS b = new BlockUVS(1, 15);
                        //BlockUVS b = new(2, 15);

                        var frontCheck = !zOutMax && (z + 1 >= size && w.GetChunk(new Vector3(x + posX, y + posY, z + 1 + posZ)).blocks[x, y, 0] == 0);
                        var backCheck = !zOutMin && (z - 1 < 0 && w.GetChunk(new Vector3(x + posX, y + posY, z - 1 + posZ)).blocks[x, y, size - 1] == 0);
                        var rightCheck = !xOutMax && (x + 1 >= size && w.GetChunk(new Vector3(x + 1 + posX, y + posY, z + posZ)).blocks[0, y, z] == 0);
                        var leftCheck = !xOutMin && (x - 1 < 0 && w.GetChunk(new Vector3(x - 1 + posX, y + posY, z + posZ)).blocks[size - 1, y, z] == 0);
                        
                        if ((!(z + 1 >= size) && chunck.blocks[x, y, z + 1] == 0) || frontCheck)
                        {
                            CreateBlockSide(BlockSide.Front, x, y, z, b);
                        }
                        if ((!(z - 1 < 0) && chunck.blocks[x, y, z - 1] == 0) || backCheck)
                        {
                            CreateBlockSide(BlockSide.Back, x, y, z, b);
                        }
                        if ((!(x + 1 >= size) && chunck.blocks[x + 1, y, z] == 0) || rightCheck)
                        {
                            CreateBlockSide(BlockSide.Right, x, y, z, b);
                        }
                        if ((!(x - 1 < 0) && chunck.blocks[x - 1, y, z] == 0) || leftCheck)
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

    internal Mesh CreateVehicleMesh(ref VehicleComponent component)
    {
        Mesh mesh = new();

        vertices.Clear();
        triangulos.Clear();
        uvs.Clear();

        float posX = component.pos.x;
        float posY = component.pos.y;
        float posZ = component.pos.z;
        int vehicleSize = component.size;

        for (int x = 0; x < component.size; x++)
        {
            for (int y = 0; y < component.size; y++)
            {
                for (int z = 0; z < component.size; z++)
                {
                    byte ID = component.blocks[x][y][z];
                    if (ID > 0)
                    {
                        var b = BlockUVS.GetBlock(ID);

                        if (z + 1 >= vehicleSize || component.blocks[x][y][z + 1] == 0)
                        {
                            CreateBlockSide(BlockSide.Front, x, y, z, b);
                        }
                        if ((z - 1 < 0 || component.blocks[x][y][z - 1] == 0))
                        {
                            CreateBlockSide(BlockSide.Back, x, y, z, b);
                        }
                        if ((x + 1 >= vehicleSize || component.blocks[x + 1][y][z] == 0))
                        {
                            CreateBlockSide(BlockSide.Right, x, y, z, b);
                        }
                        if ((x - 1 < 0 || component.blocks[x - 1][y][z] == 0))
                        {
                            CreateBlockSide(BlockSide.Left, x, y, z, b);
                        }
                        if (y + 1 >= vehicleSize || component.blocks[x][y + 1][z] == 0)
                        {
                            CreateBlockSide(BlockSide.Top, x, y, z, b);
                        }
                        if (y - 1 < 0 || component.blocks[x][y - 1][z] == 0)
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

    internal Mesh UpdateVehicleMesh(ref VehicleComponent component)
    {
        vertices.Clear();
        triangulos.Clear();
        uvs.Clear();

        Mesh mesh = new();
        mesh.Clear();

        int vehicleSize = component.size;

        for (int x = 0; x < component.size; x++)
        {
            for (int y = 0; y < component.size; y++)
            {
                for (int z = 0; z < component.size; z++)
                {
                    byte ID = component.blocks[x][y][z];
                    if (ID > 0)
                    {
                        var b = BlockUVS.GetBlock(ID);
                        
                        if (z + 1 >= vehicleSize || component.blocks[x][y][z + 1] == 0)
                        {
                            CreateBlockSide(BlockSide.Front, x, y, z, b, ref component);
                        }
                        if ((z - 1 < 0 || component.blocks[x][y][z - 1] == 0))
                        {
                            CreateBlockSide(BlockSide.Back, x, y, z, b, ref component);
                        }
                        if ((x + 1 >= vehicleSize || component.blocks[x + 1][y][z] == 0))
                        {
                            CreateBlockSide(BlockSide.Right, x, y, z, b, ref component);
                        }
                        if ((x - 1 < 0 || component.blocks[x - 1][y][z] == 0))
                        {
                            CreateBlockSide(BlockSide.Left, x, y, z, b, ref component);
                        }
                        if (y + 1 >= vehicleSize || component.blocks[x][y + 1][z] == 0)
                        {
                            CreateBlockSide(BlockSide.Top, x, y, z, b, ref component);
                        }
                        if (y - 1 < 0 || component.blocks[x][y - 1][z] == 0)
                        {
                            CreateBlockSide(BlockSide.Bottom, x, y, z, b, ref component);
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
            if (noiseValue > 0.35f)
                return 2;

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
        List<Vector3> verticesBottom = new()
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

    void CreateBlockSide(BlockSide side, int x, int y, int z, BlockUVS b, ref VehicleComponent vehicle)
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

        vertices.Add(new Vector3(x + vrtx[0].x + vehicle.meshOffset.x, y + vrtx[0].y + vehicle.meshOffset.y, z + vrtx[0].z + vehicle.meshOffset.z)); // 1
        vertices.Add(new Vector3(x + vrtx[1].x + vehicle.meshOffset.x, y + vrtx[1].y + vehicle.meshOffset.y, z + vrtx[1].z + vehicle.meshOffset.z)); // 2
        vertices.Add(new Vector3(x + vrtx[2].x + vehicle.meshOffset.x, y + vrtx[2].y + vehicle.meshOffset.y, z + vrtx[2].z + vehicle.meshOffset.z)); // 3
        vertices.Add(new Vector3(x + vrtx[3].x + vehicle.meshOffset.x, y + vrtx[3].y + vehicle.meshOffset.y, z + vrtx[3].z + vehicle.meshOffset.z)); // 4

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
}
