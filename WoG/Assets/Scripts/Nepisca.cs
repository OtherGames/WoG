using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Nepisca : MonoBehaviour
{
    public Material mNone;
    public Material mHave;
    public GameObject markerPrefab;
    public int gridRange = 7;

    Chunck chunck;

    [SerializeField]
    float min = -0.5f;
    [SerializeField]
    float max = 0.5f;

    //[SerializeField]
    List<byte> inputSpace = new List<byte>();
    [SerializeField]
    List<byte> inputTargetSpace = new List<byte>();

    bool actionInProgress;
    bool isFall;
    bool isJump;

    //===========================
    float[,] weightsHidden;
    float[] thresoldsHidden;
    float[] weightedSumsHidden;
    float[] outputsHidden;
    float[,] weightsOutputLayer;

    float[] outputs;
    float[] thresoldsOutputLayer;
    float[] weightedSumsOutput;

    int countHiddenNeuron;
    int p;
    //===========================

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);

        chunck = FindObjectOfType<Chunck>();

        FindObjectOfType<Land>().transform.position -= new Vector3(1, 0, 1);

        GenerateGrid(new Vector3(20, 103, 7));
        GenerateTargetInputSpace();

        inputSpace.AddRange(inputTargetSpace);

        //=======================
        countHiddenNeuron = inputSpace.Count * 3;
        p = inputSpace.Count;

        weightsHidden = new float[countHiddenNeuron, p];
        thresoldsHidden = new float[countHiddenNeuron];
        outputsHidden = new float[countHiddenNeuron];
        weightsOutputLayer = new float[11, countHiddenNeuron];
        weightedSumsHidden = new float[countHiddenNeuron];
        outputs = new float[11];
        thresoldsOutputLayer = new float[11];
        weightedSumsOutput = new float[11];

        StartCoroutine(Magic());
    }

    IEnumerator Magic()
    {
        // инициализация весов и порогов
        for (int i = 0; i < countHiddenNeuron; i++)
        {
            for (int j = 0; j < p; j++)
            {
                weightsHidden[i, j] = Random.Range(min, max);
            }

            thresoldsHidden[i] = Random.Range(min, max);
        }

        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < countHiddenNeuron; j++)
            {
                weightsOutputLayer[i, j] = Random.Range(min, max);
            }

            thresoldsOutputLayer[i] = Random.Range(min, max);
        }

        yield return null;

        while (true)
        {
            yield return null;

            while (isFall || actionInProgress || isJump)
            {
                yield return null;
            }

            yield return null;
            yield return null;

            while (isFall || actionInProgress || isJump)
            {
                yield return null;
            }

            // считаем выходные значения скрытого слоя
            for (int i = 0; i < countHiddenNeuron; i++)
            {
                float wSum = 0;
                for (int j = 0; j < p; j++)
                {
                    float x = inputSpace[j];
                    float w = weightsHidden[i, j];
                    wSum += w * x;
                }
                wSum -= thresoldsHidden[i];
                weightedSumsHidden[i] = wSum;
                outputsHidden[i] = ReLUActivation(wSum);
            }
            // считаем выходные значения выходного слоя
            for (int i = 0; i < 11; i++)
            {
                float wSum = 0;
                for (int j = 0; j < countHiddenNeuron; j++)
                {
                    float x = outputsHidden[j];
                    float w = weightsOutputLayer[i, j];
                    wSum += w * x;
                }
                wSum -= thresoldsOutputLayer[i];
                weightedSumsOutput[i] = wSum;
                outputs[i] = wSum;
            }

            // Смотрим что получилось на выходе
            var maxV = outputs.Max();

            for (int i = 0; i < outputs.Length; i++)
            {
                if(Mathf.Approximately(maxV, outputs[i]))
                {
                    ChooseAction(i, out var reward);
                }
            }

            yield return new WaitForSeconds(0.17f);

            UpdateCurrentSpace();

            yield return new WaitForSeconds(0.17f);

            // Корректировка весов
        }
    }

    public void ChooseAction(int i, out int reward)
    {
        reward = 0;

        if(i == 0)
        {
            ActionMoveOneBlockForward();
        }
        if (i == 1)
        {
            ActionMoveOneBlockBack();
        }
        if (i == 2)
        {
            ActionRotateLeft();
        }
        if (i == 3)
        {
            ActionRotateRight();
        }
        if (i == 4)
        {
            PlaceBottomBlock();
        }
        if (i == 5)
        {
            PlaceMiddleBlock();
        }
        if (i == 6)
        {
            PlaceTopBlock();
        }
        if (i == 7)
        {
            DestroyBottomBlock();
        }
        if (i == 8)
        {
            DestroyMiddleBlock();
        }
        if (i == 9)
        {
            DestroyTopBlock();
        }
        if (i == 10)
        {
            Jump();
        }
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

                    var marker = Instantiate(markerPrefab, pos, Quaternion.identity);
                    marker.GetComponentInChildren<TMPro.TMP_Text>().text = inputSpace.Count.ToString();

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
                            marker.GetComponent<MeshRenderer>().sharedMaterial = new Material(mHave);
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

    void GenerateTargetInputSpace()
    {
        for (int i = 0; i < inputSpace.Count; i++)
        {
            inputTargetSpace.Add(inputSpace[i]);

            if(i == 120 || i == 121 || i == 169 || i == 218)
            {
                inputTargetSpace[i] = 1;
            }
        }
    }

    void UpdateCurrentSpace()
    {
        var center = new Vector3(20, 103, 7);
        List<byte> updatedSpace = new List<byte>();

        for (int x = -gridRange; x < gridRange + 1; x++)
        {
            for (int y = -gridRange; y < gridRange + 1; y++)
            {
                for (int z = -gridRange; z < gridRange + 1; z++)
                {
                    Vector3 pos = new Vector3(x + center.x, y + center.y, z + center.z);

                    var voxel = chunck.voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                    Vector3 playerPos = transform.position;
                    playerPos.x = Mathf.RoundToInt(playerPos.x);
                    playerPos.y = Mathf.RoundToInt(playerPos.y);
                    playerPos.z = Mathf.RoundToInt(playerPos.z);
                    if (pos == playerPos)
                    {
                        updatedSpace.Add(8);
                    }
                    else
                    {
                        if (voxel != 0)
                        {
                            updatedSpace.Add(1);
                            
                        }
                        else
                        {
                            updatedSpace.Add(0);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < updatedSpace.Count; i++)
        {
            inputSpace[i] = updatedSpace[i];
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
            actionInProgress = true;
            for (int i = 0; i < 90; i++)
            {
                transform.Rotate(new Vector3(0, -1, 0));

                yield return null;
            }
            actionInProgress = false;
            //transform.rotation = Quaternion.Euler(newRot);
        }
    }

    public void ActionRotateRight()
    {
        StartCoroutine(Rotation());

        IEnumerator Rotation()
        {
            actionInProgress = true;
            for (int i = 0; i < 90; i++)
            {
                transform.Rotate(new Vector3(0, 1, 0));

                yield return null;
            }
            actionInProgress = false;
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
            isJump = true;
            while (Vector3.Distance(transform.position, newPos) > 0.1f)
            {
                yield return null;

                transform.position += 3 * Time.deltaTime * (transform.up + transform.forward);
            }

            transform.position = newPos;

            yield return null;

            isJump = false;
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
        if (chunck && !isFall && !isJump)
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(1);
        }
    }

    public static float ReLUActivation(float weightedSum)
    {
        if (weightedSum > 0)
            return weightedSum;
        else
            return 0.01f;
    }
}
