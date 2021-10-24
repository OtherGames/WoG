using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public GameObject highlight;
    public LayerMask lm;

    Land world;

    // Start is called before the first frame update
    void Start()
    {
        world = FindObjectOfType<Land>();   
    }

    // Update is called once per frame
    void Update()
    {
        BlockController();
    }

    void BlockController()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 7f, lm))
        {
            Vector3 normalPos = hit.point - (hit.normal / 2);

            int x = Mathf.FloorToInt(normalPos.x);
            int y = Mathf.FloorToInt(normalPos.y);
            int z = Mathf.FloorToInt(normalPos.z);

            Vector3 blockPosition = new Vector3(x, y, z);

            highlight.transform.position = blockPosition;
            //}

            if (Input.GetMouseButtonDown(0))
            {
                var chunck = FindObjectOfType<Chunck>();
                chunck.EditVoxel(blockPosition, 0);
            }
            if (Input.GetMouseButtonDown(1))
            {
                var chunck = FindObjectOfType<Chunck>();
                chunck.EditVoxel(blockPosition + hit.normal, 1);
            }

        }
        else
        {
            highlight.transform.position = default;
        }
    }
}
