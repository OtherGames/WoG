using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabsHolder : MonoBehaviour
{
    public GameObject morkva;
    public GameObject coal;
    public GameObject ingotIron;
    public GameObject saltpeter;
    public GameObject sulfur;
    public GameObject gunpowder;
    public GameObject ironPart;

    public GameObject Get(byte id)
    {
        switch (id)
        {
            case ITEMS.COAL:
                return coal;

            case ITEMS.GUNPOWDER:
                return gunpowder;

            case ITEMS.IRON_PART:
                return ironPart;
        }

        return null;
    }
}
