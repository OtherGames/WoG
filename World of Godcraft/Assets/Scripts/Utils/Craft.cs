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
        #region Доски
        { new byte?[] { null, 8 }, new(11, 4) },

        { new byte?[] { 8, null }, new(11, 4) },

        { new byte?[] { null, null,
                          8,  null }, new(11, 4) },

        { new byte?[] {   8,  null,  
                        null, null }, new(11, 4) },

        { new byte?[] { null,  8,
                        null, null }, new(11, 4) },

        { new byte?[] { null, null,
                        null,   8  }, new(11, 4) },
        #endregion

        #region Простой Верстак
        { new byte?[] { 11, 11 }, new(100, 1) },

        { new byte?[] {  11,   11, 
                        null, null }, new(100, 1) },

        { new byte?[] { null, null,
                         11,   11  }, new(100, 1) },

        #endregion

        #region Верстак
        { new byte?[] { 100, 100, 
                        100, 100 }, new(101, 1) },
        #endregion

        #region Простой Верстак
        #endregion

        #region Простой Верстак
        #endregion
    };


}
