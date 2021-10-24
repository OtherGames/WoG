using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunck : MonoBehaviour
{
    public ChunkCoord coord;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider col;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    Land world;

    public bool isVoxelMapPopulated = false;

    public void Init(Land world, ChunkCoord coord)
    {
        this.world = world;
        this.coord = coord;

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        col = gameObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.material;
        transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        transform.SetParent(world.transform);

        gameObject.name = "Chunk " + coord.x + ", " + coord.z;

        PopulateVoxelMap();
        UpdateChunk();
    }

    void PopulateVoxelMap()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + transform.position);

                }
            }
        }

        isVoxelMapPopulated = true;

    }

    void UpdateChunk()
    {

        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    if (world.blocktypes[voxelMap[x, y, z]].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

        CreateMesh();

    }

    void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        col.sharedMesh = mesh;
    }

    void UpdateMeshData(Vector3 pos)
    {

        for (int p = 0; p < 6; p++)
        {

            if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
            {

                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                AddTexture(world.blocktypes[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;

            }
        }

    }

    void AddTexture(int textureID)
    {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));


    }

    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;

    }
    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.CheckForVoxel(pos + transform.position);

        return world.blocktypes[voxelMap[x, y, z]].isSolid;

    }

    public void EditVoxel(Vector3 pos, byte newID)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(transform.position.x);
        zCheck -= Mathf.FloorToInt(transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newID;

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();

    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {

        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {

            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {

                world.GetChunkFromVector3(currentVoxel + transform.position).UpdateChunk();

            }

        }

    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(transform.position.x);
        zCheck -= Mathf.FloorToInt(transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];

    }

    void ClearMeshData()
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

    }


}

public class ChunkCoord
{

    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;

    }

    public bool Equals(ChunkCoord other)
    {

        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;

    }

}
