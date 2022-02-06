using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EbantropStosha : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;
    [SerializeField]
    LineRenderer lineOutput;

    int countOutputNeuron = 11;
    int countHiddenNeuron_1;
    int countHiddenNeuron_2;
    int countHiddenNeuron_3;
    int countHiddenNeuron_4;
    int p;

    [SerializeField]
    float a = 0.1f;
    [SerializeField]
    float min = -0.5f;
    [SerializeField]
    float max = 0.5f;

    //[SerializeField]
    //List<float> weights = new List<float>();
    [SerializeField]
    float[] etalons;

    [SerializeField]
    float[] outputs;
    //float threshold;

    float[,] weightsHidden_1;
    float[,] weightsHidden_2;
    float[,] weightsHidden_3;
    float[,] weightsHidden_4;
    float[,] weightsOutputLayer;
   
    float[] weightedSumsHidden_1;
    float[] weightedSumsHidden_2;
    float[] weightedSumsHidden_3;
    float[] weightedSumsHidden_4;
    float[] weightedSumsOutput;

    float[] outputsHidden_1;
    float[] outputsHidden_2;
    float[] outputsHidden_3;
    float[] outputsHidden_4;

    [SerializeField]
    float[] thresoldsHidden_1;
    [SerializeField]
    float[] thresoldsHidden_2;
    [SerializeField]
    float[] thresoldsHidden_3;
    [SerializeField]
    float[] thresoldsHidden_4;
    [SerializeField]
    float[] thresoldsOutputLayer;
    //float thresoldOutputLayer;

    List<SaveData> savesos = new List<SaveData>();

    Nepisca nepisca;

    private IEnumerator Start()
    {
        nepisca = FindObjectOfType<Nepisca>();

        yield return new WaitForSeconds(1.9f);

        savesos = JsonXyeson.LoadData();

        countHiddenNeuron_1 = savesos[0].inputishe.Count / 2;
        countHiddenNeuron_2 = countHiddenNeuron_1;
        countHiddenNeuron_3 = countHiddenNeuron_1;
        countHiddenNeuron_4 = countHiddenNeuron_1;
        p = savesos[0].inputishe.Count;

        weightsHidden_1 = new float[countHiddenNeuron_1, p];
        weightsHidden_2 = new float[countHiddenNeuron_2, countHiddenNeuron_1];
        weightsHidden_3 = new float[countHiddenNeuron_3, countHiddenNeuron_2];
        weightsHidden_4 = new float[countHiddenNeuron_4, countHiddenNeuron_3];
        thresoldsHidden_1 = new float[countHiddenNeuron_1];
        thresoldsHidden_2 = new float[countHiddenNeuron_2];
        thresoldsHidden_3 = new float[countHiddenNeuron_3];
        thresoldsHidden_4 = new float[countHiddenNeuron_4];
        thresoldsOutputLayer = new float[countOutputNeuron];
        outputsHidden_1 = new float[countHiddenNeuron_1];
        outputsHidden_2 = new float[countHiddenNeuron_2];
        outputsHidden_3 = new float[countHiddenNeuron_3];
        outputsHidden_4 = new float[countHiddenNeuron_4];
        weightsOutputLayer = new float[countOutputNeuron, countHiddenNeuron_4];
        weightedSumsHidden_1 = new float[countHiddenNeuron_1];
        weightedSumsHidden_2 = new float[countHiddenNeuron_2];
        weightedSumsHidden_3 = new float[countHiddenNeuron_3];
        weightedSumsHidden_4 = new float[countHiddenNeuron_4];
        weightedSumsOutput = new float[countOutputNeuron];

        etalons = new float[countOutputNeuron];

        outputs = new float[countOutputNeuron];

        lineRenderer.positionCount = savesos.Count;
        lineOutput.positionCount = savesos.Count;
        for (int i = 0; i < savesos.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(i, savesos[i].actionIndex));
        }

        //=============================
        for (int i = 0; i < countOutputNeuron; i++)
        {
            for (int j = 0; j < countHiddenNeuron_4; j++)
            {
                weightsOutputLayer[i,j] = Random.Range(min, max);
            }
            thresoldsOutputLayer[i] = Random.Range(min, max);
        }
        

        for (int i = 0; i < countHiddenNeuron_1; i++)
        {
            for (int j = 0; j < p; j++)
            {
                weightsHidden_1[i, j] = Random.Range(min, max);
            }

            thresoldsHidden_1[i] = Random.Range(min, max);
            
        }

        for (int i = 0; i < countHiddenNeuron_2; i++)
        {
            for (int j = 0; j < countHiddenNeuron_1; j++)
            {
                weightsHidden_2[i, j] = Random.Range(min, max);
            }
            thresoldsHidden_2[i] = Random.Range(min, max);
        }

        for (int i = 0; i < countHiddenNeuron_3; i++)
        {
            for (int j = 0; j < countHiddenNeuron_2; j++)
            {
                weightsHidden_3[i, j] = Random.Range(min, max);
            }
            thresoldsHidden_3[i] = Random.Range(min, max);
        }

        for (int i = 0; i < countHiddenNeuron_4; i++)
        {
            for (int j = 0; j < countHiddenNeuron_3; j++)
            {
                weightsHidden_4[i, j] = Random.Range(min, max);
            }
            thresoldsHidden_4[i] = Random.Range(min, max);
        }
        // ==========================================

        int iter = 0;
        while (teaching)
        {
            iter++;
            yield return null;

            float[] allErrors = new float[savesos.Count];

            for (int iSample = 0; iSample < savesos.Count; iSample++)
            {
                int stoshaIndex = Random.Range(0, savesos.Count);
                var stoshaSample = savesos[stoshaIndex];

                for (int i = 0; i < countHiddenNeuron_1; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < p; j++)
                    {
                        float x = stoshaSample.inputishe[j];
                        wSum += weightsHidden_1[i, j] * x;
                    }
                    wSum -= thresoldsHidden_1[i];
                    weightedSumsHidden_1[i] = wSum;
                    outputsHidden_1[i] = ReLUActivation(wSum);
                    //outputsHidden_1[i] = Sigmoda(wSum);
                }

                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < countHiddenNeuron_1; j++)
                    {
                        float x = outputsHidden_1[j];
                        wSum += weightsHidden_2[i, j] * x;
                    }
                    wSum -= thresoldsHidden_2[i];
                    weightedSumsHidden_2[i] = wSum;
                    outputsHidden_2[i] = ReLUActivation(wSum);
                    //outputsHidden_2[i] = Sigmoda(wSum);
                }

                for (int i = 0; i < countHiddenNeuron_3; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < countHiddenNeuron_2; j++)
                    {
                        float x = outputsHidden_2[j];
                        wSum += weightsHidden_3[i, j] * x;
                    }
                    wSum -= thresoldsHidden_3[i];
                    weightedSumsHidden_3[i] = wSum;
                    outputsHidden_3[i] = ReLUActivation(wSum);
                    //outputsHidden_2[i] = Sigmoda(wSum);
                }
                // Ебана жесть 4-ый скрытый слой...
                for (int i = 0; i < countHiddenNeuron_4; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < countHiddenNeuron_3; j++)
                    {
                        float x = outputsHidden_3[j];
                        wSum += weightsHidden_4[i, j] * x;
                    }
                    wSum -= thresoldsHidden_4[i];
                    weightedSumsHidden_4[i] = wSum;
                    outputsHidden_4[i] = ReLUActivation(wSum);
                    //outputsHidden_2[i] = Sigmoda(wSum);
                }

                // Вычисление выходных значений выходного слоя
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    float weightedSumOutputLayer = 0;
                    for (int j = 0; j < countHiddenNeuron_4; j++)
                    {
                        float x = outputsHidden_4[j];
                        weightedSumOutputLayer += weightsOutputLayer[i, j] * x;
                    }
                    weightedSumOutputLayer -= thresoldsOutputLayer[i];
                    weightedSumsOutput[i] = weightedSumOutputLayer;
                    outputs[i] = Sigmoda(weightedSumOutputLayer);//weightedSumOutputLayer;
                }

                var maxOut = outputs.Max();
                int actionIndex = 0;
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    if (Mathf.Approximately(maxOut, outputs[i]))
                    {
                        actionIndex = i;
                    }
                }

                lineOutput.SetPosition(stoshaIndex, new Vector3(stoshaIndex, actionIndex));

                yield return null;
                // Изминение порогов и весовых коэффициентов
             
                int actIdx = (int)stoshaSample.actionIndex;
                
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    if(i == actIdx)
                    {
                        if (actionIndex == actIdx)
                        {
                            etalons[i] = outputs[i];
                        }
                        else
                        {
                            etalons[i] = 0.98f;
                        }
                    }
                    else
                    {
                        etalons[i] = 0.95f;//Mathf.Clamp(outputs[i], 0f, 0.93f);
                    }
                }

                // Вычисение ошибок для выходного слоя
                float[] errsOutput = new float[countOutputNeuron];
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    errsOutput[i] = outputs[i] - etalons[i];
                }

                // Вычисеие для четвертого?!.. скрытного слоя
                float[] errHiden_4 = new float[countHiddenNeuron_4];
                for (int i = 0; i < countHiddenNeuron_4; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < countOutputNeuron; j++)
                    {
                        errSum += errsOutput[j] * DerivativeReLU(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
                        //errSum += errsOutput[j] * DerivativeSigmodos(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
                    }
                    errHiden_4[i] = errSum;
                }

                // Вычисеие для третьего скрытного слоя
                float[] errHiden_3 = new float[countHiddenNeuron_3];
                for (int i = 0; i < countHiddenNeuron_3; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < countHiddenNeuron_4; j++)
                    {
                        errSum += errHiden_4[j] * DerivativeReLU(weightedSumsHidden_4[j]) * weightsHidden_4[j, i];
                        //errSum += errsOutput[j] * DerivativeSigmodos(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
                    }
                    errHiden_3[i] = errSum;
                }

                // Вычисеие для второго скрытного слоя
                float[] errHiden_2 = new float[countHiddenNeuron_2];
                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < countHiddenNeuron_3; j++)
                    {
                        errSum += errHiden_3[j] * DerivativeReLU(weightedSumsHidden_3[j]) * weightsHidden_3[j, i];
                        //errSum += errsOutput[j] * DerivativeSigmodos(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
                    }
                    errHiden_2[i] = errSum;
                }

                // Вычисение для первого скрытно ёбыря
                float[] errHiden_1 = new float[countHiddenNeuron_1];
                for (int i = 0; i < countHiddenNeuron_1; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < countHiddenNeuron_2; j++)
                    {
                        //errSum += errHiden_2[j] * DerivativeSigmodos(weightedSumsHidden_2[j]) * weightsHidden_2[j, i];
                        errSum += errHiden_2[j] * DerivativeReLU(weightedSumsHidden_2[j]) * weightsHidden_2[j, i];
                    }
                    errHiden_1[i] = errSum;
                }


                //if(iter > 500)
                //{
                //    a = 0.0005f;
                //}

                

                // Корректировка весов от скрытого слоя к выходному i -> j
                //yield return null;
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    for (int j = 0; j < countHiddenNeuron_4; j++)
                    {
                        float x = outputsHidden_4[j];
                        float w = weightsOutputLayer[i,j] - a * errsOutput[i] * x * DerivativeSigmodos(weightedSumsOutput[i]);
                        weightsOutputLayer[i,j] = w;
                    }
                    thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errsOutput[i] * DerivativeSigmodos(weightedSumsOutput[i]);
                }

                for (int i = 0; i < countHiddenNeuron_4; i++)
                {
                    for (int j = 0; j < countHiddenNeuron_3; j++)
                    {
                        float w = weightsHidden_4[i, j];
                        float x = outputsHidden_3[j];
                        w = w - a * errHiden_4[i] * DerivativeReLU(weightedSumsHidden_4[i]) * x;

                        weightsHidden_4[i, j] = w;
                    }
                    thresoldsHidden_4[i] = thresoldsHidden_4[i] + a * errHiden_4[i] * DerivativeReLU(weightedSumsHidden_4[i]);
                }

                for (int i = 0; i < countHiddenNeuron_3; i++)
                {
                    for (int j = 0; j < countHiddenNeuron_2; j++)
                    {
                        float w = weightsHidden_3[i, j];
                        float x = outputsHidden_2[j];
                        w = w - a * errHiden_3[i] * DerivativeReLU(weightedSumsHidden_3[i]) * x;
                        
                        weightsHidden_3[i, j] = w;
                    }
                    thresoldsHidden_3[i] = thresoldsHidden_3[i] + a * errHiden_3[i] * DerivativeReLU(weightedSumsHidden_3[i]);
                }

                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    for (int j = 0; j < countHiddenNeuron_1; j++)
                    {
                        float w = weightsHidden_2[i, j];
                        float x = outputsHidden_1[j];
                        w = w - a * errHiden_2[i] * DerivativeReLU(weightedSumsHidden_2[i]) * x;
                        //w = w - a * errHiden_2[i] * DerivativeSigmodos(weightedSumsHidden_2[i]) * x;
                        weightsHidden_2[i, j] = w;
                    }
                    thresoldsHidden_2[i] = thresoldsHidden_2[i] + a * errHiden_2[i] * DerivativeReLU(weightedSumsHidden_2[i]);
                    //thresoldsHidden_2[i] = thresoldsHidden_2[i] + a * errHiden_2[i] * DerivativeSigmodos(weightedSumsHidden_2[i]);
                }

                // Корректировка весов от входного слоя к скрытому
                for (int i = 0; i < countHiddenNeuron_1; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        float w = weightsHidden_1[i, j];
                        float x = stoshaSample.inputishe[j];
                        w = w - a * errHiden_1[i] * DerivativeReLU(weightedSumsHidden_1[i]) * x;
                        //w = w - a * errHiden_1[i] * DerivativeSigmodos(weightedSumsHidden_1[i]) * x;
                        weightsHidden_1[i, j] = w;
                    }
                    thresoldsHidden_1[i] = thresoldsHidden_1[i] + a * errHiden_1[i] * DerivativeReLU(weightedSumsHidden_1[i]);
                    //thresoldsHidden_1[i] = thresoldsHidden_1[i] + a * errHiden_1[i] * DerivativeSigmodos(weightedSumsHidden_1[i]);
                }

                //yield return null;

                // Суммарная ошибка сети
                float sumErr = 0;
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    sumErr += errsOutput[i] * errsOutput[i];
                }
                allErrors[iSample] = sumErr;
            }

            float MEGA_ERROR = 0;
            for (int i = 0; i < savesos.Count; i++)
            {
                MEGA_ERROR += allErrors[i];
            }
            MEGA_ERROR *= 0.5f;

            print($"Ошибкос {MEGA_ERROR} итерация - {iter}");
        }
    }

    IEnumerator MegaCheker()
    {
        yield return new WaitForSeconds(3f);


        for (int iSample = 0; iSample < savesos.Count; iSample++)
        {
            for (int i = 0; i < countHiddenNeuron_1; i++)
            {
                float wSum = 0;
                for (int j = 0; j < p; j++)
                {
                    float x = nepisca.inputSpace[j];
                    wSum += weightsHidden_1[i, j] * x;
                }
                wSum -= thresoldsHidden_1[i];
                weightedSumsHidden_1[i] = wSum;
                //outputsHidden[i] = ReLUActivation(wSum);
                outputsHidden_1[i] = Sigmoda(wSum);
            }

            for (int i = 0; i < countHiddenNeuron_2; i++)
            {
                float wSum = 0;
                for (int j = 0; j < countHiddenNeuron_1; j++)
                {
                    float x = outputsHidden_1[j];
                    wSum += weightsHidden_2[i, j] * x;
                }
                wSum -= thresoldsHidden_2[i];
                weightedSumsHidden_2[i] = wSum;
                outputsHidden_2[i] = Sigmoda(wSum);
            }

            // Вычисление выходных значений выходного слоя
            for (int i = 0; i < countOutputNeuron; i++)
            {
                float weightedSumOutputLayer = 0;
                for (int j = 0; j < countHiddenNeuron_2; j++)
                {
                    float x = outputsHidden_2[j];
                    weightedSumOutputLayer += weightsOutputLayer[i, j] * x;
                }
                weightedSumOutputLayer -= thresoldsOutputLayer[i];
                weightedSumsOutput[i] = weightedSumOutputLayer;
                outputs[i] = Sigmoda(weightedSumOutputLayer);//weightedSumOutputLayer;
            }

            var maxOut = outputs.Max();
            int actionIndex = 0;
            for (int i = 0; i < countOutputNeuron; i++)
            {
                if (Mathf.Approximately(maxOut, outputs[i]))
                {
                    actionIndex = i;
                }
            }

            lineOutput.SetPosition(iSample, new Vector3(iSample, actionIndex));

            nepisca.ChooseAction(actionIndex, out var empty);
            print(actionIndex + " ### " + iSample);
            yield return new WaitForSeconds(1.7f);

            nepisca.UpdateCurrentSpace();
        }

    }
    //===================================================================================
    bool teaching = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            teaching = false;
            StartCoroutine(MegaCheker());
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            a += 0.01f;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            a -= 0.01f;
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            a += 0.0001f;
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            a -= 0.0001f;
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            a += 0.000001f;
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            a -= 0.000001f;
        }
    }

    float Sigmoda(float weightedSum)
    {
        var denominator = 1 + Mathf.Exp(1 - weightedSum);
        var numerator = 1;
        return numerator / denominator;
    }

    float DerivativeSigmodos(float weightedSum)
    {
        return Sigmoda(weightedSum) * (1 - Sigmoda(weightedSum));
    }

    float ReLUActivation(float weightedSum)
    {
        if (weightedSum > 0)
            return weightedSum;
        else
            return 0.001f * weightedSum;
    }

    float DerivativeReLU(float weightedSum)
    {
        if (weightedSum > 0)
            return 1;
        else
            return 0.001f;
    }
}