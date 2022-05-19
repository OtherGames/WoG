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
    public GameObject bullet;
    public GameObject magazine;
    public GameObject silicon;
    public GameObject simplePistol;

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

            case ITEMS.BULLET:
                return bullet;

            case ITEMS.MAGAZINE:
                return magazine;

            case ITEMS.SILICON:
                return silicon;

            case ITEMS.SIMPLE_PISTOL:
                return simplePistol;
        }

        return null;
    }
}
