using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    public byte quickSlotID = 1;


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            quickSlotID = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            quickSlotID = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            quickSlotID = 3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            quickSlotID = 4;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            quickSlotID = 5;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            quickSlotID = 6;
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            quickSlotID = 7;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            quickSlotID = 8;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            quickSlotID = 9;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            quickSlotID = 10;
        }

        if(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.I))
        {

        }
    }
}
