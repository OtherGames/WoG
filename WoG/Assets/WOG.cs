using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Physics;

using Random = UnityEngine.Random;

public class WOG : MonoBehaviour//SystemBase
{
    [SerializeField] UnityEngine.Material material;

    bool[,,] map = new bool[size, size, size];
    Dictionary<BlockSide, List<int3>> blockVerticesSet;
    Dictionary<BlockSide, List<int>> blockTrianglesSet;

    const int size = 38;
    int ebota = 130;

    public List<int3> chunks;

    void CreateChunk(int posX, int posY, int posZ)
    {
        
        vertices?.Clear();
        triangulos?.Clear();

        natVertices?.Clear();
        natTriangles?.Clear();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    map[x, y, z] = BlockExist(x + posX, y + posY, z + posZ);
                }
            }
        }

        var EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype entityArchetype = EntityManager.CreateArchetype
        (
            typeof(RenderBounds),
            typeof(WorldRenderBounds),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(PerInstanceCullingTag),
            typeof(Rotation),
            typeof(BlendProbeTag)
        );

        NativeArray<Entity> entityArray = new NativeArray<Entity>(1, Allocator.Temp);

        EntityManager.CreateEntity(entityArchetype, entityArray);

        foreach (var entity in entityArray)
        {
            Mesh mesh = GenerateMesh(posX, posY, posZ);
            float3 pos = new float3(posX, posY, posZ);
            EntityManager.SetComponentData(entity, new Translation { Value = pos });
            EntityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = material,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true,
                needMotionVectorPass = true,
            });
            EntityManager.SetComponentData(entity, new RenderBounds
            {
                Value = new AABB { Center = mesh.bounds.center, Extents = mesh.bounds.extents }
            });

            var meshCol = Unity.Physics.MeshCollider.Create(natVertices.ToNativeArray(Allocator.Temp), natTriangles.ToNativeArray(Allocator.Temp));
            var col = new PhysicsCollider { Value = meshCol };
            var aabb = meshCol.Value.CalculateAabb();

            EntityManager.SetName(entity, $"Chunk {posX},{posY},{posZ}");


            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1u << 10,
                CollidesWith = 1u << 11
            };


            EntityManager.AddComponentData(entity, col);
        }

        entityArray.Dispose();
        
        chunks.Add(new int3(posX, posY, posZ));
        //print($"{posX} {posY} {posZ} {chunks.Count}");
    }

    protected void Start()
    {
        chunks = new List<int3>();
        //for (int x = 0; x < size; x++)
        //{
        //    for (int y = 0; y < size; y++)
        //    {
        //        float3 pos = new float3(x + offset.x, y + offset.y, x + offset.x);
        //        float noiseX = Mathf.Abs((float)(pos.x + offset.x) / ebota);
        //        float noiseY = Mathf.Abs((float)(pos.y + offset.y) / ebota);
        //        float noiseZ = Mathf.Abs((float)(pos.z + offset.z) / ebota);

        //        float res = SimplexNoise.Noise.Generate(noiseX, noiseY, noiseZ);
        //    }
        //}
        


        blockVerticesSet = new Dictionary<BlockSide, List<int3>>();//new NativeKeyValueArrays<BlockSide, NativeArray<int3>>(6, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        blockTrianglesSet = new Dictionary<BlockSide, List<int>>();

        List<int3> verticesFront = new List<int3>
        {
            new int3( 0, 0, 1 ),
            new int3(-1, 0, 1 ),
            new int3(-1, 1, 1 ),
            new int3( 0, 1, 1 ),
        };
        List<int3> verticesBack = new List<int3>
        {
            new int3( 0, 0, 0 ),
            new int3(-1, 0, 0 ),
            new int3(-1, 1, 0 ),
            new int3( 0, 1, 0 ),
        };
        List<int3> verticesRight = new List<int3>
        {
            new int3( 0, 0, 0 ),
            new int3( 0, 0, 1 ),
            new int3( 0, 1, 1 ),
            new int3( 0, 1, 0 ),
        };
        List<int3> verticesLeft = new List<int3>
        {
            new int3(-1, 0, 0 ),
            new int3(-1, 0, 1 ),
            new int3(-1, 1, 1 ),
            new int3(-1, 1, 0 ),
        };
        List<int3> verticesTop = new List<int3>
        {
            new int3( 0, 1, 0 ),
            new int3(-1, 1, 0 ),
            new int3(-1, 1, 1 ),
            new int3( 0, 1, 1 ),
        };
        List<int3> verticesBottom = new List<int3>
        {
            new int3( 0, 0, 0 ),
            new int3(-1, 0, 0 ),
            new int3(-1, 0, 1 ),
            new int3( 0, 0, 1 ),
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
        blockVerticesSet[BlockSide.Bottom] = verticesBottom;//.ToNativeArray(Allocator.Persistent);

        InitTriangulos();

        CreateChunk(0, 0, 0);
        //natVertices.Dispose();
        //natTriangles.Dispose();

    }

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangulos = new List<int>();

    List<float3> natVertices;
    List<int3> natTriangles;

    Mesh GenerateMesh(int posX, int posY, int posZ)
    {
        List<Vector2> uvs = new List<Vector2>();
        natTriangles = new List<int3>();
        natVertices = new List<float3>();

        Mesh mesh = new Mesh();
        mesh.Clear();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (map[x, y, z])
                    {
                        
                        if ((z + 1 >= size && !BlockExist(x+posX, y+posY, z + 1 + posZ)) || (!(z + 1 >= size) && map[x, y, z + 1] == false))
                        {
                            CreateBlockSide(BlockSide.Front, x, y, z);
                        }
                        if ((z - 1 < 0 && !BlockExist(x+posX, y+posY, z - 1 + posZ)) || (!(z - 1 < 0) && map[x, y, z - 1] == false))
                        {
                            CreateBlockSide(BlockSide.Back, x, y, z);
                        }
                        if ((x + 1 >= size && !BlockExist(x+1+posX,y+posY,z+posZ)) || (!(x + 1 >= size) && map[x + 1, y, z] == false))
                        {
                            CreateBlockSide(BlockSide.Right, x, y, z);
                        }
                        if ((x - 1 < 0 && !BlockExist(x-1+posX,y+posY,z+posZ)) || (!(x - 1 < 0) && map[x - 1, y, z] == false))
                        {
                            CreateBlockSide(BlockSide.Left, x, y, z);
                        }
                        if (!(y + 1 >= size) && map[x, y + 1, z] == false || y + 1 >= size)
                        {
                            CreateBlockSide(BlockSide.Top, x, y, z);
                        }
                        if (!(y - 1 < 0) && map[x, y - 1, z] == false)
                        {
                            CreateBlockSide(BlockSide.Bottom, x, y, z);
                        }
                        //if (!(z + 1 >= size) && map[x, y, z + 1] == false)
                        //{
                        //    CreateBlockSide(BlockSide.Front, x, y, z);
                        //}
                        //if (!(z - 1 < 0) && map[x, y, z - 1] == false)
                        //{
                        //    CreateBlockSide(BlockSide.Back, x, y, z);
                        //}
                        //if (!(x + 1 >= size) && map[x + 1, y, z] == false)
                        //{
                        //    CreateBlockSide(BlockSide.Right, x, y, z);
                        //}
                        //if (!(x - 1 < 0) && map[x - 1, y, z] == false)
                        //{
                        //    CreateBlockSide(BlockSide.Left, x, y, z);
                        //}
                        //if (!(y + 1 >= size) && map[x, y + 1, z] == false || y+1 >= size)
                        //{
                        //    CreateBlockSide(BlockSide.Top, x, y, z);
                        //}
                        //if (!(y - 1 < 0) && map[x, y - 1, z] == false)
                        //{
                        //    CreateBlockSide(BlockSide.Bottom, x, y, z);
                        //}
                    }
                }
            }
        }

        

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangulos.ToArray();
        //mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.OptimizeReorderVertexBuffer();
        mesh.Optimize();

        return mesh;
    }

    void CreateBlockSide(BlockSide side, int x, int y, int z)
    {
        List<int3> vrtx = blockVerticesSet[side];
        List<int> trngls = blockTrianglesSet[side];
        int offset = 1;

        triangulos.Add(trngls[0] - offset + vertices.Count);
        triangulos.Add(trngls[1] - offset + vertices.Count);
        triangulos.Add(trngls[2] - offset + vertices.Count);

        triangulos.Add(trngls[3] - offset + vertices.Count);
        triangulos.Add(trngls[4] - offset + vertices.Count);
        triangulos.Add(trngls[5] - offset + vertices.Count);
        natTriangles.Add(new int3
        (
            trngls[0] - offset + vertices.Count,
            trngls[1] - offset + vertices.Count,
            trngls[2] - offset + vertices.Count
        ));
        natTriangles.Add(new int3
        (
            trngls[3] - offset + vertices.Count,
            trngls[4] - offset + vertices.Count,
            trngls[5] - offset + vertices.Count
        ));

        vertices.Add(new Vector3(x + vrtx[0].x, y + vrtx[0].y, z + vrtx[0].z)); // 1
        vertices.Add(new Vector3(x + vrtx[1].x, y + vrtx[1].y, z + vrtx[1].z)); // 2
        vertices.Add(new Vector3(x + vrtx[2].x, y + vrtx[2].y, z + vrtx[2].z)); // 3
        vertices.Add(new Vector3(x + vrtx[3].x, y + vrtx[3].y, z + vrtx[3].z)); // 4

        natVertices.Add(new float3(x + vrtx[0].x, y + vrtx[0].y, z + vrtx[0].z));
        natVertices.Add(new float3(x + vrtx[1].x, y + vrtx[1].y, z + vrtx[1].z)); // 2
        natVertices.Add(new float3(x + vrtx[2].x, y + vrtx[2].y, z + vrtx[2].z)); // 3
        natVertices.Add(new float3(x + vrtx[3].x, y + vrtx[3].y, z + vrtx[3].z)); // 4

    }
   
    public struct Block
    {
        


    }

    

    void InitTriangulos()
    {
        List<int> trianglesFront  = new List<int> { 3, 2, 1, 4, 3, 1 };
        List<int> trianglesBack   = new List<int> { 1, 2, 3, 1, 3, 4 };
        List<int> trianglesRight  = new List<int> { 1, 3, 2, 4, 3, 1 };
        List<int> trianglesLeft   = new List<int> { 2, 3, 1, 1, 3, 4 };
        List<int> trianglesTop    = new List<int> { 1, 2, 3, 1, 3, 4 };
        List<int> trianglesBottom = new List<int> { 3, 2, 1, 4, 3, 1 };

        blockTrianglesSet.Add(BlockSide.Front, trianglesFront);
        blockTrianglesSet.Add(BlockSide.Back, trianglesBack);
        blockTrianglesSet.Add(BlockSide.Right, trianglesRight);
        blockTrianglesSet.Add(BlockSide.Left, trianglesLeft);
        blockTrianglesSet.Add(BlockSide.Top, trianglesTop);
        blockTrianglesSet.Add(BlockSide.Bottom, trianglesBottom);
    }

    private void Update()
    {
        Transform cam = Camera.main.transform;
        int viewRange = 3;

        int x = (int)cam.position.x;
        int y = (int)cam.position.y;
        int z = (int)cam.position.z;

        //y = Mathf.Clamp(y, 0, size * 2);
        
        for (int i = -viewRange; i < viewRange; i++)
        {
            for (int j = -viewRange; j < viewRange; j++)
            {
                //for (int k = 0; k < 1; k++)
                //{
                    if (!ChunkExist(x + (j * size), y, z + (i * size)))
                    {
                        int zz = Mathf.FloorToInt((float)z / size) * size;
                        int xx = Mathf.FloorToInt((float)x / size) * size;
                        int yy = Mathf.FloorToInt((float)y / size) * size;

                        //CreateChunk(xx + (j * size), yy + (k*size), zz + (i * size));
                        CreateChunk(xx + (j * size), 0, zz + (i * size));
                    }
                //}
            }
        }
        
    }

    public bool ChunkExist(int x, int y, int z)
    {
        foreach (var chunk in chunks)
        {
            if (Mathf.Approximately(z, chunk.z) || Mathf.Approximately(x, chunk.x))// || Mathf.Approximately(y, chunk.y))
                return true;

            if (z >= chunk.z + size || z < chunk.z || x >= chunk.x + size || x < chunk.x)// || y >= chunk.y + size || y < chunk.y)
                continue;
            
            return true;
        }
        return false;
    }

    public bool BlockExist(int x, int y, int z)
    {
        Random.InitState(505);
        //Random.InitState(888);
        int k = 1000000;
        Vector3 offset = new Vector3(Random.value * k, Random.value * k, Random.value * k);

        float3 pos = new float3
        (
            x + offset.x,
            y + offset.y,
            z + offset.z
        );
        float noiseX = Mathf.Abs((float)(pos.x + offset.x) / ebota);
        float noiseY = Mathf.Abs((float)(pos.y + offset.y) / ebota);
        float noiseZ = Mathf.Abs((float)(pos.z + offset.z) / ebota);
#pragma warning disable CS0436 // Тип конфликтует с импортированным типом
        var res = noise.snoise(new float3(noiseX, noiseY, noiseZ));//snoise(pos);
#pragma warning restore CS0436 // Тип конфликтует с импортированным типом
        //print(res);
        //res += (8 - y) / 7;
        //res /= y / 17f;

        if (y < 3) res = 0.5f;

        if (res > 0.3f)
        {
            //map[x, y, z] = true;
            return true;
        }

        return false;
    }
    //protected override void OnUpdate()
    //{
    //    // Assign values to local variables captured in your job here, so that it has
    //    // everything it needs to do its work when it runs later.
    //    // For example,
    //    //     float deltaTime = Time.DeltaTime;

    //    // This declares a new kind of job, which is a unit of work to do.
    //    // The job is declared as an Entities.ForEach with the target components as parameters,
    //    // meaning it will process all entities in the world that have both
    //    // Translation and Rotation components. Change it to process the component
    //    // types you want.



    //    Entities.ForEach((ref Translation translation, in Rotation rotation) => {
    //        // Implement the work to perform for each entity here.
    //        // You should only access data that is local or that is a
    //        // field on this job. Note that the 'rotation' parameter is
    //        // marked as 'in', which means it cannot be modified,
    //        // but allows this job to run in parallel with other jobs
    //        // that want to read Rotation component data.
    //        // For example,
    //        //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
    //    }).Schedule();
    //}
}
