using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nepisca : MonoBehaviour
{
    public Material mNone;
    public Material mHave;
    public GameObject markerPrefab;
    public int gridRange = 7;

    Chunck chunck;

    [SerializeField]
    List<byte> inputSpace = new List<byte>();

    bool actionInProgress;
    bool isFall;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);

        chunck = FindObjectOfType<Chunck>();

        FindObjectOfType<Land>().transform.position -= new Vector3(1, 0, 1);

        GenerateGrid(transform.position - new Vector3(0, 1, 0)); 
    }

    void GenerateGrid(Vector3 center)
    {
        center.x = Mathf.FloorToInt(center.x);
        center.y = Mathf.FloorToInt(center.y);
        center.z = Mathf.FloorToInt(center.z);

        for (int x = -gridRange; x < gridRange + 1; x++)
        {
            for (int y = -gridRange; y < gridRange + 1; y++)
            {
                for (int z = -gridRange; z < gridRange + 1; z++)
                {
                    Vector3 pos = new Vector3(x + center.x, y + center.y, z + center.z);

                    //var marker = Instantiate(markerPrefab, pos, Quaternion.identity);

                    var voxel = chunck.voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                    if (pos == center)
                    {
                        inputSpace.Add(8);
                    }
                    else
                    {
                        if (voxel != 0)
                        {
                            inputSpace.Add(1);
                            //marker.GetComponent<MeshRenderer>().sharedMaterial = new Material(mHave);
                        }
                        else
                        {
                            inputSpace.Add(0);
                        }
                    }

                    
                }
            }
        }
    }

    #region Ставить Блоки
    public void PlaceMiddleBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 1, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);
        chunck.EditVoxel(blockPos, 1);
    }

    public void PlaceTopBlock()
    {
        Vector3 blockPos = transform.position + transform.forward;
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);
        chunck.EditVoxel(blockPos, 1);
    }

    public void PlaceBottomBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 2, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);
        chunck.EditVoxel(blockPos, 1);
    }
    #endregion

    #region Убирать Блоки
    public void DestroyMiddleBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 1, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);
        chunck.EditVoxel(blockPos, 0);
    }

    public void DestroyTopBlock()
    {
        Vector3 blockPos = transform.position + transform.forward;
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);
        chunck.EditVoxel(blockPos, 0);
    }

    public void DestroyBottomBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 2, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);
        chunck.EditVoxel(blockPos, 0);
    }
    #endregion

    public void ActionMoveOneBlockForward()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos + transform.forward;

        StartCoroutine(Action());

        IEnumerator Action()
        {
            actionInProgress = true;
            Vector2 v1 = new Vector2(newPos.x, newPos.z);
            Vector2 v2 = new Vector2(transform.position.x, transform.position.z);

            while (Vector2.Distance(v1, v2) > 0.05f)
            {
                yield return null;

                transform.position += transform.forward * Time.deltaTime;

                v2 = new Vector2(transform.position.x, transform.position.z);
   
            }

            actionInProgress = false;
        }
    }

    public void ActionMoveOneBlockBack()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos - transform.forward;

        StartCoroutine(Action());

        IEnumerator Action()
        {
            actionInProgress = true;
            Vector2 v1 = new Vector2(newPos.x, newPos.z);
            Vector2 v2 = new Vector2(transform.position.x, transform.position.z);

            while (Vector2.Distance(v1, v2) > 0.05f)
            {
                yield return null;

                transform.position -= transform.forward * Time.deltaTime;

                v2 = new Vector2(transform.position.x, transform.position.z);
            }

            actionInProgress = false;

        }
    }

    public void ActionRotateLeft()
    {
        //Vector3 newRot = transform.rotation.eulerAngles - new Vector3(0, 90, 0);

        StartCoroutine(Rotation());

        IEnumerator Rotation()
        {
            //Vector3 curRot = transform.rotation.eulerAngles;

            for (int i = 0; i < 90; i++)
            {
                transform.Rotate(new Vector3(0, -1, 0));

                yield return null;
            }
           
            //transform.rotation = Quaternion.Euler(newRot);
        }
    }

    public void ActionRotateRight()
    {
        StartCoroutine(Rotation());

        IEnumerator Rotation()
        {
            for (int i = 0; i < 90; i++)
            {
                transform.Rotate(new Vector3(0, 1, 0));

                yield return null;
            }
        }
    }

    public void Jump()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos + transform.up + transform.forward;

        StartCoroutine(Action());

        IEnumerator Action()
        {
            isFall = true;
            while (Vector3.Distance(transform.position, newPos) > 0.1f)
            {
                yield return null;

                transform.position += 3 * Time.deltaTime * (transform.up + transform.forward);
            }

            transform.position = newPos;
            isFall = false;
        }
    }

    bool CheckIsGrounded()
    {
        Vector3 pos = transform.position - new Vector3(0, 2, 0);// - transform.forward;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        pos.z = Mathf.RoundToInt(pos.z);

        var block = chunck.voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        if(block != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Fall()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos - transform.up;

        StartCoroutine(Action());

        IEnumerator Action()
        {
            isFall = true;
            while (transform.position.y - newPos.y > 0.05f)
            {
                yield return null;

                transform.position -= 3 * Time.deltaTime * transform.up;
            }

            transform.position = newPos;
            isFall = false;
        }
    }

    
    private void Update()
    {
        if (chunck && !isFall)
        {
            if (!CheckIsGrounded())
            {
                Fall();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ActionMoveOneBlockForward();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActionRotateLeft();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActionRotateRight();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActionMoveOneBlockBack();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlaceMiddleBlock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlaceTopBlock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            PlaceBottomBlock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            DestroyMiddleBlock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            DestroyTopBlock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            DestroyBottomBlock();
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Jump();
        }
    }
}
