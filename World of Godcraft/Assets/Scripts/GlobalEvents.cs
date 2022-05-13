using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class GlobalEvents 
{
    public class ItemTaked : UnityEvent { }

    public static ItemTaked itemTaked = new();

    //-----------------------------------------------------------------------

    public class ItemRemoved : UnityEvent<int> { }

    public static ItemRemoved itemUsing = new();

    //-----------------------------------------------------------------------

    public class InteractBlockHited : UnityEvent<byte> { }

    public static InteractBlockHited interactBlockHited = new();

    //-----------------------------------------------------------------------

}
