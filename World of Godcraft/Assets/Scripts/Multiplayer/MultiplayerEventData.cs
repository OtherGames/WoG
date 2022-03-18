using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HealthPointNetworkData
{
    public int viewId;
    public int healthPoint;
}

public class DirectionNetworkData
{
    public Vector2 value;
    public int viewID;
}

public class WeaponDependencyNetworkData
{
    public int weaponViewID;
    public int playerViewID;
}

public class AttackLongRangeNetworkData
{
    public int weaponViewID;
    public int playerViewID;
}

