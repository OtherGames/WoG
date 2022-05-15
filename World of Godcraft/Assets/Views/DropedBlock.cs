using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropedBlock : MonoBehaviour
{
    
    void Update()
    {
        // HOT FIX
        if (gameObject.layer != 5)
        {
            transform.Rotate(Vector3.up, 1f);
        }
    }


}
