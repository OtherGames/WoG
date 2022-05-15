using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craft
{
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

        #region Печь
        {
            new byte?[] { 3,  3,  3,
                          3, null,3,
                          3,  3,  3 }, new(102, 1)
        },
        #endregion

        #region Простой Верстак
        #endregion
    };

    // id блока/предмета и время горения
    public Dictionary<byte, byte> setsCombustible = new()
    {
        { 8,  39 },
        { 11, 10 },
    };

    // id блока/предмета и время обработки огнем
    public Dictionary<byte, byte> setsFurnaceable = new()
    {

    };
}
