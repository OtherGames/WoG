using UnityEngine;

struct VehicleHitEvent
{
    public Vector3Int blockPos;
    public Vector3Int connectedPos;
    public Vector3 globalPos;
    public Vector3 globalRot;
    public byte blockID;
    public int entityVehicle;
}
