using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropedBlock : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        // HOT FIX
        if (gameObject.layer != 5)
        {
            transform.Rotate(Vector3.up, 1f);
        }
    }


}
