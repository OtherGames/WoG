using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ITEMS 
{
    public const byte SILICON = 160;
    public const byte MAGAZINE = 161;
    public const byte BULLET = 162;
    public const byte INGOT_IRON = 163;
    public const byte SULFUR = 165;
    public const byte COAL = 166;
    public const byte SALTPETER = 167;
    public const byte SIMPLE_PISTOL = 168;
    public const byte MORKVA = 170;
    public const byte GUNPOWDER = 173;
    public const byte IRON_PART = 200;
    

    public static bool IsCombustible(byte id)
    {
        if (id == COAL || id == 8 | id == 11 | id == 100)
            return true;
        else
            return false;
    }
}
