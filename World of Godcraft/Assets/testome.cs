using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class testome : MonoBehaviour
{

    Vector3 newPos = new(10, 13, 30);

    bool sasf;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            sasf = true;

        if (sasf)
            transform.position = Vector3.MoveTowards(transform.position, newPos, Time.deltaTime * 10);
    }


    
}
