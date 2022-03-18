using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    public byte blockID = 1;


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            blockID = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            blockID = 2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            blockID = 3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            blockID = 4;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            blockID = 5;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            blockID = 6;
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            blockID = 7;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            blockID = 8;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            blockID = 9;
        }
    }
}
