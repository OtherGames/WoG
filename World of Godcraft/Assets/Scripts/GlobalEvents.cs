using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class GlobalEvents 
{
    public class ItemTaked : UnityEvent { }

    public static ItemTaked itemTaked = new();

    //-----------------------------------------------------------------------

}
