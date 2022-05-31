using System.Collections.Generic;
using UnityEngine;

struct ActuatorComponent
{
    public MeshRenderer renderer;
    public MeshFilter meshFilter;
    public Vector3 meshOffset;
    public Vector3 pos;
    public int size;

    public Dictionary<Vector3, GameObject> colliders;

    public List<List<List<byte>>> blocks;
}
