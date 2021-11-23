using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Nepisca : MonoBehaviour
{
    public bool zapiskoAction = false;
    public LineRenderer lineEtalons;
    public LineRenderer lineOutput;
    public Material mNone;
    public Material mHave;
    public GameObject markerPrefab;
    public int gridRange = 7;

    Chunck chunck;

    [SerializeField]
    float min = -0.5f;
    [SerializeField]
    float max = 0.5f;

    [SerializeField]
    public List<float> inputSpace = new List<float>();
    //[SerializeField]
    List<float> inputTargetSpace = new List<float>();

    List<SaveData> savesos = new List<SaveData>();

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
    float[] softmax;
    static float[] thresoldsOutputLayer;
    [SerializeField]
    float[] weightedSumsOutput;

    int countHiddenNeuron;
    int p;
    float playerValue = 3f;
    float rotateValue = 1f;
    float voxelValue = 1f;
    //===========================
    List<int> actionHistory = new List<int>();
    [SerializeField]
    List<PassData[]> passDatas = new List<PassData[]>();
    int countActions = 0;
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
        inputSpace.Add(rotateValue);
        inputSpace.Add(0);
        inputSpace.Add(0);
        inputSpace.Add(0);

        //=======================
        countHiddenNeuron = inputSpace.Count * 5;
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
        softmax = new float[11];
        weightedSumsOutput = new float[11];

        

        if (!zapiskoAction)
        {
            savesos = JsonXyeson.LoadData();
            StartCoroutine(S_EbychimYchitelem());
            //StartCoroutine(Magic());
        }
        
    }

    IEnumerator S_EbychimYchitelem()
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
        yield return null;

        lineEtalons.positionCount = savesos.Count;
        lineOutput.positionCount = savesos.Count;

        for (int i = 0; i < savesos.Count; i++)
        {
            lineEtalons.SetPosition(i, new Vector3(i, savesos[i].actionIndex));
        }

        int countiter = 0;
        while (teaching)
        {
            yield return null;

            for (int iSample = 0; iSample < savesos.Count; iSample++)
            {
                countiter++;
                
                // ������� �������� �������� �������� ����
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < p; j++)
                    {
                        float x = savesos[iSample].inputishe[j];
                        float w = weightsHidden[i, j];
                        wSum += w * x;
                    }
                    wSum -= thresoldsHidden[i];
                    weightedSumsHidden[i] = wSum;
                    outputsHidden[i] = ReLUActivation(wSum);
                }
                // ������� �������� �������� ��������� ����
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
                // ������� softmax
                for (int i = 0; i < 11; i++)
                {
                    softmax[i] = outputs[i];//SoftmaxActivation(i);
                }

                // ===== ������� ��� ���������� �� ������ ======
                // ������� ������ ������������� ��������
                var maxOut = softmax.Max();
                int actionIndex = 0;
                for (int i = 0; i < 11; i++)
                {
                    if (System.Math.Abs(maxOut - softmax[i]) < 0.00000001)
                    {
                        //softmax[i] = 0.9f;
                        //softmax[i] = 1f;
                        actionIndex = i;
                    }
                    else
                    {
                        //softmax[i] = 0.01f;
                        //softmax[i] = 0f;
                    }
                }

                //yield return new WaitForSeconds(0.17f);
                yield return new WaitForSeconds(0.01f);
                lineOutput.SetPosition(iSample, new Vector3(iSample, actionIndex));

                // ===== ������������� ����� =====
                float[] etalons = new float[11];
                for (int i = 0; i < 11; i++)
                {
                    if(savesos[iSample].actionIndex == i)
                    {
                        //etalons[i] = 0.9f;
                        etalons[i] = 1f;
                    }
                    else
                    {
                        //etalons[i] = 0.01f;
                        etalons[i] = 0f;
                    }
                }

                // ������ ��������� ���� � ���� ������� �� ��������
                float[] errOutput_J = new float[11];
                float[] derivititi = new float[11];
                float errOutSum = 0;
                //for (int i = 0; i < 11; i++)
                //{
                //    errOutSum += (softmax[i] - etalons[i]) * softmax[i] * (1 - softmax[i]);
                //    derivititi[i] = errOutSum;
                //}
                //for (int i = 0; i < 11; i++)
                //{
                //    errOutSum += (softmax[i] - etalons[i]) * softmax[i] * (1 - softmax[i]);
                //}
                //for (int i = 0; i < 11; i++)
                //{
                //    derivititi[i] = errOutSum;
                //    //derivititi[i] = (softmax[i] - etalons[i]) * softmax[i] * (1 - softmax[i]);
                //}
                //print((0 - 1) * 0 * (1 - 0));
                print(countiter + " ������  " + errOutSum + " -=-= " + weightsOutputLayer[0, 0] + " ##### " + weightsHidden[0, 0]);

                for (int i = 0; i < 11; i++)
                {
                    errOutput_J[i] = softmax[i] - etalons[i];
                }


                // ������ �������� ����
                float[] errHiden_I = new float[countHiddenNeuron];
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < 11; j++)
                    {
                        //errSum += errOutput_J[j] * weightsOutputLayer[j, i];
                        errSum += errOutput_J[j] * weightsOutputLayer[j, i] * DerivativeReLU(weightedSumsOutput[j]);
                        //errSum += errOutput_J[j] * weightsOutputLayer[j, i] * softmax[j] * (1 - softmax[j]);
                        //errHiden_I[i] = errOutput_J[j] * weightsOutputLayer[j, i] * DerivativeReLU(weightedSumsOutput[j]);
                    }
                    errHiden_I[i] = errSum;
                }

                float a = 0.001f;
                // ������������� ����� �� �������� ���� � ��������� i -> j

                for (int i = 0; i < 11; i++)
                {
                    for (int j = 0; j < countHiddenNeuron; j++)
                    {
                        float x = outputsHidden[j];
                        //float w = weightsOutputLayer[i, j] - a * x * errOutput_J[i];// * (float)DerivativeSoftmax(i)/*DerivativeReLU(weightedSumsOutput[i])/**/ 
                        //float w = weightsOutputLayer[i, j] - a * x * derivititi[i];
                        //float w = weightsOutputLayer[i, j] - a * x * errOutput_J[i] * softmax[i] * (1 - softmax[i]);//DerivativeSoftmax(i);
                        float w = weightsOutputLayer[i, j] - a * x * errOutput_J[i] * DerivativeReLU(weightedSumsOutput[i]);
                        weightsOutputLayer[i, j] = w;
                    }

                    //thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutput_J[i];// * (float)DerivativeSoftmax(i);//(weightedSumsOutput[i]);
                    //thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * derivititi[i];
                    //thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutput_J[i] * softmax[i] * (1 - softmax[i]);//DerivativeSoftmax(i);
                    thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutput_J[i] * DerivativeReLU(weightedSumsOutput[i]);
                }

                //yield return new WaitForSeconds(0.17f);

                // ������������� ����� �� �������� ���� � ��������
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        float w = weightsHidden[i, j];
                        float x = savesos[iSample].inputishe[j];
                        w = w - a * errHiden_I[i] * DerivativeReLU(weightedSumsHidden[i]) * x;
                        weightsHidden[i, j] = w;
                    }

                    thresoldsHidden[i] = thresoldsHidden[i] + a * errHiden_I[i] * DerivativeReLU(weightedSumsHidden[i]);
                }

                // ���� �����
                bool check = true;
                for (int i = 0; i < savesos.Count; i++)
                {
                    if (lineEtalons.GetPosition(i) != lineOutput.GetPosition(i))
                    {
                        check = false;
                        break;
                    }
                }
                if (check)
                {
                    teaching = false;
                }
            }
        }
    }

    bool teaching = true;

    IEnumerator MegaCheker()
    {
        yield return new WaitForSeconds(3f);

        
            //yield return new WaitForSeconds(0.17f);

        for (int iSample = 0; iSample < savesos.Count; iSample++)
        {
            // ������� �������� �������� �������� ����
            for (int i = 0; i < countHiddenNeuron; i++)
            {
                float wSum = 0;
                for (int j = 0; j < p; j++)
                {
                    float x = savesos[iSample].inputishe[j];
                    float w = weightsHidden[i, j];
                    wSum += w * x;
                }
                wSum -= thresoldsHidden[i];
                weightedSumsHidden[i] = wSum;
                outputsHidden[i] = ReLUActivation(wSum);
            }
            // ������� �������� �������� ��������� ����
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
            // ������� softmax
            for (int i = 0; i < 11; i++)
            {
                softmax[i] = outputs[i];//SoftmaxActivation(i);
            }

            // ===== ������� ��� ���������� �� ������ ======
            // ������� ������ ������������� ��������
            var maxOut = softmax.Max();
            int actionIndex = 0;
            for (int i = 0; i < 11; i++)
            {
                if (System.Math.Abs(maxOut - softmax[i]) < 0.00000001)
                {
                    actionIndex = i;
                    break;
                }
            }

            lineOutput.SetPosition(iSample, new Vector3(iSample, actionIndex));
            ChooseAction(actionIndex, out var empty);
            print(actionIndex + " ### " + iSample);
            yield return new WaitForSeconds(1.7f);

            UpdateCurrentSpace();
        }
        
    }
    //===================================================================================

    IEnumerator Magic()
    {
        //print(0.9f - (-1));
        // ������������� ����� � �������

        if (Mathf.Approximately(weightsHidden[0, 7], 0))// ����� ����� ��������
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
        else
        {
            print("������������ ��������");
            
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

            // ������� �������� �������� �������� ����
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
            // ������� �������� �������� ��������� ����
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
            // ������� softmax
            for (int i = 0; i < 11; i++)
            {
                softmax[i] = SoftmaxActivation(i);
            }

            // ===== ������� ��� ���������� �� ������ ======
            // ������� ������ ������������� ��������
            var maxV = softmax.Max();

            int actionIndex = 0;
            float reward = 0;
            for (int i = 0; i < 11; i++)
            {
                if (System.Math.Abs(maxV - softmax[i]) < 0.00000001)
                {
                    actionIndex = i;
                    ChooseAction(i, out reward);// ��������� �������� �� ������
                    softmax[i] = 1f;
                }
                else
                {
                    softmax[i] = 0;
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

            // ���� ������ �� ������ ����
            //PassData[] datas = new PassData[11];
            //for (int i = 0; i < 11; i++)
            //{
            //    if (i == actionIndex)
            //        datas[i] = new PassData { rewardos = reward, output = softmax[i] };
            //    else
            //        datas[i] = new PassData { rewardos = 0, output = softmax[i] };

            //}
            //passDatas.Add(datas);
            //countActions++;

            //if (countActions > 30)
            //{
            float[] rewards = new float[11];
            for (int i = 0; i < 11; i++)
            {
                if (i == actionIndex)
                    if (reward < 0)
                        rewards[i] = 0;
                    else
                        rewards[i] = 0.98f;
                else if (reward < 0)
                    rewards[i] = 0.05f;
                else
                    rewards[i] = 0;
            }
            // ===== ������������� ����� =====

            // ������ ��������� ���� � ���� ������� �� ��������
            float[] errOutput_J = new float[11];
            float[] derivititi = new float[11];
            float errOutSum = 0;
            for (int i = 0; i < 11; i++)
            {
                errOutSum += (softmax[i] - rewards[i]) * softmax[i] * (1 - softmax[i]);
                derivititi[i] = errOutSum;
            }
            
            for (int i = 0; i < 11; i++)
            {
                errOutput_J[i] = softmax[i] - rewards[i];/**///errOutSum;
                //print(softmax[i] + " ===== "+ rewards[i] + " ������ - ... " + errOutput_J[i]);
            }

            
            // ������ �������� ����
            float[] errHiden_I = new float[countHiddenNeuron];
            for (int i = 0; i < countHiddenNeuron; i++)
            {
                float errSum = 0;
                for (int j = 0; j < 11; j++)
                {
                    errSum += errOutput_J[j] * weightsOutputLayer[j, i];
                }
                errHiden_I[i] = errSum;
            }

            float a = 0.15f;
            // ������������� ����� �� �������� ���� � ��������� i -> j

            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < countHiddenNeuron; j++)
                {
                    //float w = weightsOutputLayer[i, j] - a * outputsHidden[j] * errOutput_J[i];// * (float)DerivativeSoftmax(i)/*DerivativeReLU(weightedSumsOutput[i])/**/ 
                    float w = weightsOutputLayer[i, j] - a * outputsHidden[j] * derivititi[i];
                    weightsOutputLayer[i, j] = w;
                }

                //thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutput_J[i];// * (float)DerivativeSoftmax(i);//(weightedSumsOutput[i]);
                thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * derivititi[i];
            }

            yield return new WaitForSeconds(0.17f);
            // ������������� ����� �� �������� ���� � ��������

            for (int i = 0; i < countHiddenNeuron; i++)
            {
                for (int j = 0; j < p; j++)
                {
                    float w = weightsHidden[i, j];
                    float x = inputSpace[j];
                    w = w - a * errHiden_I[i] * DerivativeReLU(weightedSumsHidden[i]) * x;
                    weightsHidden[i, j] = w;
                }

                thresoldsHidden[i] = thresoldsHidden[i] + a * errHiden_I[i] * DerivativeReLU(weightedSumsHidden[i]);
            }

            #region �������
            // ��������� ������������� ����� ��� ������� ������� ���������

            // ������������� �����

            // ������ ��������� ���� � ���� ������� �� ��������
            //if (weightedSumsOutput[0] < -1000)
            //{
            //    print("������ ���");
            //    float[] errOutputCorrect = new float[11];

            //    for (int i = 0; i < errOutputCorrect.Length; i++)
            //    {
            //        errOutputCorrect[i] = -3f;
            //    }

            //    // ������ �������� ����
            //    float[] errHidenCorrect = new float[countHiddenNeuron];
            //    for (int i = 0; i < countHiddenNeuron; i++)
            //    {
            //        float errSum = 0;
            //        for (int j = 0; j < 11; j++)
            //        {
            //            errSum += errOutputCorrect[j] * DerivativeReLU(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
            //        }
            //        errHidenCorrect[i] = errSum;
            //    }


            //    // ������������� ����� �� �������� ���� � ��������� i -> j

            //    for (int i = 0; i < 11; i++)
            //    {
            //        for (int j = 0; j < countHiddenNeuron; j++)
            //        {
            //            float w = weightsOutputLayer[i, j] - a * errOutputCorrect[i] * DerivativeReLU(weightedSumsOutput[i]) * outputsHidden[j];
            //            weightsOutputLayer[i, j] = w;
            //        }

            //        thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errOutputCorrect[i] * DerivativeReLU(weightedSumsOutput[i]);

            //    }

            //    yield return new WaitForSeconds(0.17f);
            //    // ������������� ����� �� �������� ���� � ��������

            //    for (int i = 0; i < countHiddenNeuron; i++)
            //    {
            //        for (int j = 0; j < p; j++)
            //        {
            //            float w = weightsHidden[i, j];
            //            float x = inputSpace[j];
            //            w = w - a * errHidenCorrect[i] * DerivativeReLU(weightedSumsHidden[i]) * x;
            //            weightsHidden[i, j] = w;
            //        }

            //        thresoldsHidden[i] = thresoldsHidden[i] + a * errHidenCorrect[i] * DerivativeReLU(weightedSumsHidden[i]);
            //    }
            //}
            #endregion

            yield return new WaitForSeconds(.7f);

            //SceneManager.LoadScene(1);
            //}
        }
    }

    public void ChooseAction(int i, out float reward)
    {
        reward = 0.01f;

        //actionHistory.Add(i);
        //if (actionHistory.Count >= 5)
        //{
        //    actionHistory.RemoveAt(0);

        //    int firstValue = actionHistory.First();
        //    var simValue = actionHistory.FindAll(v => v == firstValue);
        //    if (simValue.Count == actionHistory.Count)
        //    {
        //        reward = -1;
        //        return;
        //    }
        //}

        if (i == 0)
        {
            if (!ActionMoveOneBlockForward())
            {
                reward = -1;
            }
        }
        if (i == 2)
        {
            if (!ActionMoveOneBlockBack())
            {
                reward = -1;
            }
        }
        if (i == 1)
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
                        return 1;
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
                        inputSpace.Add(playerValue);
                    }
                    else
                    {
                        if (voxel != 0)
                        {
                            inputSpace.Add(voxelValue);
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
            if(Mathf.Approximately(inputSpace[i], playerValue))
            {
                inputTargetSpace.Add(0);
            }
            else
            {
                inputTargetSpace.Add(inputSpace[i]);
            }
            

            if(i == 120 || i == 121 || i == 169 || i == 218)
            {
                inputTargetSpace[i] = voxelValue;
            }
        }
    }

    public void UpdateCurrentSpace()
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
                        updatedSpace.Add(playerValue);
                    }
                    else
                    {
                        if (voxel != 0)
                        {
                            updatedSpace.Add(voxelValue);
                            
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

        var rots = GetRotationInput();
        int count = inputSpace.Count;
        for (int i = 0; i < rots.Count; i++)
        {
            inputSpace[count - rots.Count + i] = rots[i];
            //print(rots[i]+ " =========================================");
        }
    }

    List<float> GetRotationInput()
    {
        List<float> rotations = new List<float>();
        if (transform.forward.z > 0.7f)
            rotations.Add(rotateValue);
        else
            rotations.Add(0f);
        if (transform.forward.z < -0.7f)
            rotations.Add(rotateValue);
        else
            rotations.Add(0f);
        if (transform.forward.x > 0.7f)
            rotations.Add(rotateValue);
        else
            rotations.Add(0f);
        if (transform.forward.x < -0.7f)
            rotations.Add(rotateValue);
        else
            rotations.Add(0f);
        
        return rotations;
    }

    #region ������� �����
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

    #region ������� �����
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

    private void CreateDataShot(int actionIndex)
    {
        float[] inputs = inputSpace.ToArray();
        SaveData sd = new SaveData
        {
            inputishe = inputs.ToList(),
            actionIndex = actionIndex,
        };
        savesos.Add(sd);
        
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.5f);

            UpdateCurrentSpace();
            print("���������� ���������..");
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
            CreateDataShot(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActionRotateLeft();
            CreateDataShot(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActionRotateRight();
            CreateDataShot(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActionMoveOneBlockBack();
            CreateDataShot(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlaceMiddleBlock();
            CreateDataShot(5);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlaceTopBlock();
            CreateDataShot(6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            PlaceBottomBlock();
            CreateDataShot(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            DestroyMiddleBlock();
            CreateDataShot(8);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            DestroyTopBlock();
            CreateDataShot(9);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            DestroyBottomBlock();
            CreateDataShot(7);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            Jump();
            CreateDataShot(10);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(1);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            JsonXyeson.SaveFile(savesos);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            teaching = false;
            StartCoroutine(MegaCheker());
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

    public float SoftmaxActivation(int sumIndex)
    {
        float numerator = Mathf.Exp(outputs[sumIndex]);
        float denominator = 0;
        for (int i = 0; i < outputs.Length; i++)
        {
            denominator += Mathf.Exp(outputs[i]);
        }
        return numerator / denominator;
    }

    public float DerivativeSoftmax(int sumIndex)
    {
        float y = SoftmaxActivation(sumIndex);
        return y * (1 - y);
    }

    [System.Serializable]
    public struct PassData
    {
        public float rewardos;
        public float output;
    }
}
