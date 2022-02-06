using UnityEngine;

struct ChunckComponent
{
    public MeshRenderer renderer;
    public MeshFilter meshFilter;
    public MeshCollider collider;
    public Vector3 pos;

    public byte[,,] blocks;
}
