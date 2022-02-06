using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public GameObject highlight;
    public LayerMask lm;


    WorldOfGodcraft godcraft;

    // Start is called before the first frame update
    void Start()
    {
        godcraft = FindObjectOfType<WorldOfGodcraft>();
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
            //int x = Mathf.FloorToInt(hit.point.x);
            //int y = Mathf.FloorToInt(hit.point.y);
            //int z = Mathf.FloorToInt(hit.point.z);

            //Vector3 blockPosition = new Vector3(x + 1, y, z);

            //highlight.transform.position = blockPosition - new Vector3(0.5f, 0.5f, -0.5f);

            Vector3 normalPos = hit.point - (hit.normal / 2);

            int x = Mathf.FloorToInt(normalPos.x);
            int y = Mathf.FloorToInt(normalPos.y);
            int z = Mathf.FloorToInt(normalPos.z);

            Vector3 blockPosition = new(x, y, z);

            highlight.transform.position = blockPosition;


            
            if (Input.GetMouseButtonDown(0))
            {
                var e = godcraft.EcsWorld.NewEntity();

                var pool = godcraft.EcsWorld.GetPool<ChunckHitEvent>();
                pool.Add(e);
                ref var component = ref pool.Get(e);
                component.collider = hit.collider;
                component.position = blockPosition;
                component.blockId = 0;

            }
            if (Input.GetMouseButtonDown(1))
            {
                //var chunck = FindObjectOfType<Chunck>();
                //chunck.EditVoxel(blockPosition + hit.normal, 1);

                var e = godcraft.EcsWorld.NewEntity();

                var pool = godcraft.EcsWorld.GetPool<ChunckHitEvent>();
                pool.Add(e);
                ref var component = ref pool.Get(e);
                component.collider = hit.collider;
                component.position = blockPosition + hit.normal;
                component.blockId = 1;
            }

        }
        else
        {
            highlight.transform.position = default;
        }
    }
}
