using System.Collections.Generic;
using UnityEngine;

struct VehicleComponent
{
    public MeshRenderer renderer;
    public MeshFilter meshFilter;
    public MeshCollider collider;
    public Vector3 pos;
    public int size;

    public List<List<List<byte>>> blocks;
}