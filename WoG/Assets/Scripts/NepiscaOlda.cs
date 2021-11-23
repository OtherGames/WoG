using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NepiscaOlda : MonoBehaviour
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
    List<float> inputSpace = new List<float>();
    [SerializeField]
    List<float> inputTargetSpace = new List<float>();

    bool actionInProgress;
    bool isFall;
    bool isJump;

    //===========================
    static float[,] weightsHidden;
    static float[] thresoldsHidden;
    [SerializeField]
    float[] weightedSumsHidden;
    float[] outputsHidden;
    static float[,] weightsOutputLayer;

    float[] outputs;
    double[] softmax;
    static float[] thresoldsOutputLayer;
    [SerializeField]
    float[] weightedSumsOutput;

    int countHiddenNeuron;
    int p;
    //===========================
    List<int> actionHistory = new List<int>();
    //===========================

    Vector3 targetPos = new Vector3(20, 103, 7);

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1.5f);

        chunck = FindObjectOfType<Chunck>();

        FindObjectOfType<Land>().transform.position -= new Vector3(1, 0, 1);

        GenerateGrid(targetPos);
        GenerateTargetInputSpace();

        inputSpace.AddRange(inputTargetSpace);

        //=======================
        countHiddenNeuron = inputSpace.Count / 2;
        p = inputSpace.Count;

        if(weightsHidden == null)
        {
            weightsHidden = new float[countHiddenNeuron, p];
            thresoldsHidden = new float[countHiddenNeuron];
            weightsOutputLayer = new float[11, countHiddenNeuron];
            thresoldsOutputLayer = new float[11];
        }

        outputsHidden = new float[countHiddenNeuron];
        weightedSumsHidden = new float[countHiddenNeuron];
        outputs = new float[11];
        softmax = new double[11];
        weightedSumsOutput = new float[11];

        StartCoroutine(Magic());

        StartCoroutine(DelayRestart());
    }

    IEnumerator DelayRestart()
    {
        yield return new WaitForSeconds(300);

        SceneManager.LoadScene(1);
    }

    IEnumerator Magic()
    {
        // инициализация весов и порогов

        if (Mathf.Approximately(weightsHidden[0, 7], 0))// Берем любое значение
        {
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
        }

        yield return null;

        while (true)
        {
            yield return null;

            while (isFall || actionInProgress || isJump)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.05f);

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
                outputs[i] = wSum;//ReLUActivation(wSum);//wSum;
            }
            // Считаем softmax
            for (int i = 0; i < 11; i++)
            {
                softmax[i] = SoftmaxActivation(i);
            }

            // ===== Смотрим что получилось на выходе ======
            // находим индекс максимального значения
            var maxV = softmax.Max();

            int actionIndex = 0;
            float reward = 0;
            for (int i = 0; i < 11; i++)
            {
                if(System.Math.Abs(maxV - softmax[i]) < 0.00000001)
                {
                    actionIndex = i;
                    ChooseAction(i, out reward);// Совершаем действие со средой
                    break;
                }
            }

            yield return new WaitForSeconds(0.17f);
            
            UpdateCurrentSpace();
            var checkReward = CheckTargetSpace();
            print(actionIndex + " # " + reward + " $ " + checkReward);
            if (checkReward > 0)
            {
                reward = checkReward;
            }
            yield return new WaitForSeconds(0.17f);

            // ===== Корректировка весов =====

            // Ошибки выходного слоя в виде награды за действия
            float[] errOutput = new float[11];
            errOutput[actionIndex] = reward * -1;
            // Ошибки скрытого слоя
            float[] errHiden = new float[countHiddenNeuron];
            for (int i = 0; i < countHiddenNeuron; i++)
            {
                float errSum = 0;
                for (int j = 0; j < 11; j++)
                {
                    errSum += errOutput[j] * DerivativeReLU(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
                }
                errHiden[i] = errSum;
            }

            float a = 0.05f;
            // Корректировка весов от скрытого слоя к выходному i -> j
            
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < countHiddenNeuron; j++)
                {
                    float w = weightsOutputLayer[i, j] - a * errOutput[i] * DerivativeReLU(weightedSumsOutput[i]) * outputsHidden[j];
                    weightsOutputLayer[i, j] = w;
                }

                thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutput[i] * DerivativeReLU(weightedSumsOutput[i]);

            }

            yield return new WaitForSeconds(0.17f);
            // Корректировка весов от входного слоя к скрытому

            for (int i = 0; i < countHiddenNeuron; i++)
            {
                for (int j = 0; j < p; j++)
                {
                    float w = weightsHidden[i, j];
                    float x = inputSpace[j];
                    w = w - a * errHiden[i] * DerivativeReLU(weightedSumsHidden[i]) * x;
                    weightsHidden[i, j] = w;
                }

                thresoldsHidden[i] = thresoldsHidden[i] + a * errHiden[i] * DerivativeReLU(weightedSumsHidden[i]);
            }

            // ПОВТОРНАЯ КОРРЕКТИРОВКА ВЕСОВ ПРИ СЛИШКОМ БОЛЬШИХ ЗНАЧЕНИЯХ

            // Корректировка весов

            // Ошибки выходного слоя в виде награды за действия
            if (weightedSumsOutput[0] < -1000)
            {
                print("Ебашим вес");
                float[] errOutputCorrect = new float[11];

                for (int i = 0; i < errOutputCorrect.Length; i++)
                {
                    errOutputCorrect[i] = -3f;
                }

                // Ошибки скрытого слоя
                float[] errHidenCorrect = new float[countHiddenNeuron];
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < 11; j++)
                    {
                        errSum += errOutputCorrect[j] * DerivativeReLU(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
                    }
                    errHidenCorrect[i] = errSum;
                }

                
                // Корректировка весов от скрытого слоя к выходному i -> j

                for (int i = 0; i < 11; i++)
                {
                    for (int j = 0; j < countHiddenNeuron; j++)
                    {
                        float w = weightsOutputLayer[i, j] - a * errOutputCorrect[i] * DerivativeReLU(weightedSumsOutput[i]) * outputsHidden[j];
                        weightsOutputLayer[i, j] = w;
                    }

                    thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutputCorrect[i] * DerivativeReLU(weightedSumsOutput[i]);

                }

                yield return new WaitForSeconds(0.17f);
                // Корректировка весов от входного слоя к скрытому

                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        float w = weightsHidden[i, j];
                        float x = inputSpace[j];
                        w = w - a * errHidenCorrect[i] * DerivativeReLU(weightedSumsHidden[i]) * x;
                        weightsHidden[i, j] = w;
                    }

                    thresoldsHidden[i] = thresoldsHidden[i] + a * errHidenCorrect[i] * DerivativeReLU(weightedSumsHidden[i]);
                }
            }
        }
    }

    public void ChooseAction(int i, out float reward)
    {
        reward = 0.001f;

        actionHistory.Add(i);
        if(actionHistory.Count >= 3)
        {
            actionHistory.RemoveAt(0);

            int firstValue = actionHistory.First();
            var simValue = actionHistory.FindAll(v => v == firstValue);
            if (simValue.Count == actionHistory.Count)
            {
                reward = -1;
                return;
            }
        }

        if (i == 0)
        {
            if (!ActionMoveOneBlockForward())
            {
                reward = -1;
            }
        }
        if (i == 1)
        {
            if (!ActionMoveOneBlockBack())
            {
                reward = -1;
            }
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
            if (!PlaceBottomBlock())
            {
                reward = -1;
            }
        }
        if (i == 5)
        {
            if (!PlaceMiddleBlock())
            {
                reward = -1;
            }
        }
        if (i == 6)
        {
            if (!PlaceTopBlock())
            {
                reward = -1;
            }
        }
        if (i == 7)
        {
            if (!DestroyBottomBlock())
            {
                reward = -1;
            }
        }
        if (i == 8)
        {
            if (!DestroyMiddleBlock())
            {
                reward = -1;
            }
        }
        if (i == 9)
        {
            if (!DestroyTopBlock())
            {
                reward = -1;
            }
        }
        if (i == 10)
        {
            if (!Jump())
            {
                reward = -1;
            }
        }
    }

    bool firstCheck = true;
    [SerializeField]
    List<int> addedVoxels = new List<int>();
    private int CheckTargetSpace()
    {
        List<byte> currentSpace = new List<byte>();

        for (int x = -gridRange; x < gridRange + 1; x++)
        {
            for (int y = -gridRange; y < gridRange + 1; y++)
            {
                for (int z = -gridRange; z < gridRange + 1; z++)
                {
                    Vector3 pos = new Vector3(x + targetPos.x, y + targetPos.y, z + targetPos.z);

                    var voxel = chunck.voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                    if(voxel != 0)
                    {
                        currentSpace.Add(1);
                    }
                    else
                    {
                        currentSpace.Add(0);
                    }

                }
            }
        }

        for (int i = 0; i < inputTargetSpace.Count; i++)
        {
            if(inputTargetSpace[i] == currentSpace[i])
            {
                if (!addedVoxels.Contains(i))
                {
                    addedVoxels.Add(i);

                    if (!firstCheck)
                    {
                        return 13;
                    }
                }
            }
        }

        if (firstCheck)
        {
            firstCheck = false;
        }

        return 0;
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
                        inputSpace.Add(0.5f);
                    }
                    else
                    {
                        if (voxel != 0)
                        {
                            inputSpace.Add(1f);
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
                inputTargetSpace[i] = 1f;
            }
        }
    }

    void UpdateCurrentSpace()
    {
        var center = targetPos;
        List<float> updatedSpace = new List<float>();

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
                        updatedSpace.Add(0.5f);
                    }
                    else
                    {
                        if (voxel != 0)
                        {
                            updatedSpace.Add(1f);
                            
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
    public bool PlaceMiddleBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 1, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);

        if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] != 0)
        {
            return false;
        }
        else if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y-1, (int)blockPos.z] == 0)
        {
            return false;
        }
        else
        {
            chunck.EditVoxel(blockPos, 1);
            return true;
        }
        
    }

    public bool PlaceTopBlock()
    {
        Vector3 blockPos = transform.position + transform.forward;
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);

        if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] != 0)
        {
            return false;
        }
        else if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y-1, (int)blockPos.z] == 0)
        {
            return false;
        }
        else
        {
            chunck.EditVoxel(blockPos, 1);
            return true;
        }
    }

    public bool PlaceBottomBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 2, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);

        if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] != 0)
        {
            return false;
        }
        else
        {
            chunck.EditVoxel(blockPos, 1);
            return true;
        }
    }
    #endregion

    #region Убирать Блоки
    public bool DestroyMiddleBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 1, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);

        if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == 0)
        {
            return false;
        }
        else
        {
            chunck.EditVoxel(blockPos, 0);
            return true;
        }
    }

    public bool DestroyTopBlock()
    {
        Vector3 blockPos = transform.position + transform.forward;
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);

        if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == 0)
        {
            return false;
        }
        else
        {
            chunck.EditVoxel(blockPos, 0);
            return true;
        }
        
    }

    public bool DestroyBottomBlock()
    {
        Vector3 blockPos = transform.position + transform.forward - new Vector3(0, 2, 0);
        blockPos.x = Mathf.RoundToInt(blockPos.x);
        blockPos.y = Mathf.RoundToInt(blockPos.y);
        blockPos.z = Mathf.RoundToInt(blockPos.z);

        if (chunck.voxelMap[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] == 0)
        {
            return false;
        }
        else
        {
            chunck.EditVoxel(blockPos, 0);
            return true;
        }
    }
    #endregion

    public bool ActionMoveOneBlockForward()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos + transform.forward;

        newPos.x = Mathf.RoundToInt(newPos.x);
        newPos.y = Mathf.RoundToInt(newPos.y);
        newPos.z = Mathf.RoundToInt(newPos.z);

        if (chunck.voxelMap[(int)newPos.x, (int)newPos.y-1, (int)newPos.z] != 0)
        {
            return false;
        }
        else
        {
            if(newPos.x > targetPos.x + (gridRange)
            || newPos.x < targetPos.x - (gridRange)
            || newPos.z < targetPos.z - (gridRange)
            || newPos.z > targetPos.z + (gridRange)
            || newPos.y < targetPos.y - (gridRange)
            || newPos.y > targetPos.y + (gridRange))
            {
                return false;
            }
            
            StartCoroutine(Action());
            return true;
        }

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

    public bool ActionMoveOneBlockBack()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos - transform.forward;

        newPos.x = Mathf.RoundToInt(newPos.x);
        newPos.y = Mathf.RoundToInt(newPos.y);
        newPos.z = Mathf.RoundToInt(newPos.z);

        if (chunck.voxelMap[(int)newPos.x, (int)newPos.y-1, (int)newPos.z] != 0)
        {
            return false;
        }
        else
        {
            if (newPos.x > targetPos.x + (gridRange)
            || newPos.x < targetPos.x - (gridRange)
            || newPos.z < targetPos.z - (gridRange)
            || newPos.z > targetPos.z + (gridRange)
            || newPos.y < targetPos.y - (gridRange)
            || newPos.y > targetPos.y + (gridRange))
            {
                return false;
            }

            StartCoroutine(Action());
            return true;
        }

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

    public bool Jump()
    {
        Vector3 currentPos;
        currentPos.x = Mathf.RoundToInt(transform.position.x);
        currentPos.y = Mathf.RoundToInt(transform.position.y);
        currentPos.z = Mathf.RoundToInt(transform.position.z);

        Vector3 newPos = currentPos + transform.up + transform.forward;

        newPos.x = Mathf.RoundToInt(newPos.x);
        newPos.y = Mathf.RoundToInt(newPos.y);
        newPos.z = Mathf.RoundToInt(newPos.z);

        if (chunck.voxelMap[(int)newPos.x, (int)newPos.y, (int)newPos.z] != 0)
        {
            return false;
        }
        else if (chunck.voxelMap[(int)newPos.x, (int)newPos.y+1, (int)newPos.z] != 0)
        {
            return false;
        }
        else
        {
            if (newPos.x > targetPos.x + (gridRange)
            || newPos.x < targetPos.x - (gridRange)
            || newPos.z < targetPos.z - (gridRange)
            || newPos.z > targetPos.z + (gridRange)
            || newPos.y < targetPos.y - (gridRange)
            || newPos.y > targetPos.y + (gridRange))
            {
                return false;
            }

            StartCoroutine(Action());
            return true;
        }

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
            print(ActionMoveOneBlockForward());
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

    float DerivativeReLU(float weightedSum)
    {
        if (weightedSum > 0)
            return 1;
        else
            return 0.01f;
    }

    public double SoftmaxActivation(int sumIndex)
    {
        double numerator = Mathf.Exp(outputs[sumIndex]);
        double denominator = 0;
        for (int i = 0; i < outputs.Length; i++)
        {
            denominator += Mathf.Exp(outputs[i]);
        }
        return numerator / denominator;
    }
}
