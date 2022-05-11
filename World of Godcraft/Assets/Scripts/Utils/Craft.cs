using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craft
{
    //public Dictionary<byte?[], byte> sets = new()
    //{
    //    { new byte?[] { null, 8 }, 100 },
    //    { new byte?[] { 8, null }, 100 },
    //};

    public Dictionary<byte?[], Tuple<byte, int>> sets = new()
    {
        { new byte?[] { null, 8 }, new(100, 1) },
        { new byte?[] { 8, null }, new(100, 1) },
    };


}
