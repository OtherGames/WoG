using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActuatorMeshGenerator
{
    public const float TextureOffset = 1f / 16f;
    Dictionary<BlockSide, List<Vector3>> blockVerticesSet;
    Dictionary<BlockSide, List<int>> blockTrianglesSet;

    List<Vector3> vertices = new();
    List<int> triangulos = new();
    List<Vector2> uvs = new();

    public ActuatorMeshGenerator()
    {
        blockVerticesSet = new Dictionary<BlockSide, List<Vector3>>();
        blockTrianglesSet = new Dictionary<BlockSide, List<int>>();

        DictionaryInits();
        InitTriangulos();
    }

    internal Mesh CreateMesh(ref VehicleComponent component)
    {
        Mesh mesh = new();

        vertices.Clear();
        triangulos.Clear();
        uvs.Clear();

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

        vertices.Add(new Vector3(x + vrtx[0].x + 0.5f, y + vrtx[0].y - 0.5f, z + vrtx[0].z - 0.5f)); // 1
        vertices.Add(new Vector3(x + vrtx[1].x + 0.5f, y + vrtx[1].y - 0.5f, z + vrtx[1].z - 0.5f)); // 2
        vertices.Add(new Vector3(x + vrtx[2].x + 0.5f, y + vrtx[2].y - 0.5f, z + vrtx[2].z - 0.5f)); // 3
        vertices.Add(new Vector3(x + vrtx[3].x + 0.5f, y + vrtx[3].y - 0.5f, z + vrtx[3].z - 0.5f)); // 4

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

        vertices.Add(new Vector3(x + vrtx[0].x + vehicle.meshOffset.x+0.5f, y + vrtx[0].y + vehicle.meshOffset.y-0.5f, z + vrtx[0].z + vehicle.meshOffset.z-0.5f)); // 1
        vertices.Add(new Vector3(x + vrtx[1].x + vehicle.meshOffset.x+0.5f, y + vrtx[1].y + vehicle.meshOffset.y-0.5f, z + vrtx[1].z + vehicle.meshOffset.z-0.5f)); // 2
        vertices.Add(new Vector3(x + vrtx[2].x + vehicle.meshOffset.x+0.5f, y + vrtx[2].y + vehicle.meshOffset.y-0.5f, z + vrtx[2].z + vehicle.meshOffset.z-0.5f)); // 3
        vertices.Add(new Vector3(x + vrtx[3].x + vehicle.meshOffset.x+0.5f, y + vrtx[3].y + vehicle.meshOffset.y-0.5f, z + vrtx[3].z + vehicle.meshOffset.z-0.5f)); // 4

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
            new Vector3(0, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(-1, 0, 1),
            new Vector3(0, 0, 1),
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
}
