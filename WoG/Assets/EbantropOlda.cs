using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EbantropOlda : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;
    [SerializeField]
    LineRenderer lineOutput;

    int countOutputNeuron = 11;
    int countHiddenNeuron_1;
    int countHiddenNeuron_2;
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
    float[] outputs;
    //float threshold;

    float[,] weightsHidden_1;
    float[,] weightsHidden_2;
    float[] thresoldsHidden_1;
    float[] thresoldsHidden_2;
    float[] weightedSumsHidden_1;
    float[] weightedSumsHidden_2;
    float[] outputsHidden_1;
    float[] outputsHidden_2;


    [SerializeField]
    float[] weightsOutputLayer;
    float thresoldOutputLayer;

    List<SaveData> savesos = new List<SaveData>();

    Nepisca nepisca;

    private IEnumerator Start()
    {
        nepisca = FindObjectOfType<Nepisca>();

        yield return new WaitForSeconds(1.9f);

        savesos = JsonXyeson.LoadData();

        countHiddenNeuron_1 = 80;//[0].inputishe.Count;// / 2;
        countHiddenNeuron_2 = 50;
        p = savesos[0].inputishe.Count;

        weightsHidden_1 = new float[countHiddenNeuron_1, p];
        weightsHidden_2 = new float[countHiddenNeuron_2, countHiddenNeuron_1];
        thresoldsHidden_1 = new float[countHiddenNeuron_1];
        thresoldsHidden_2 = new float[countHiddenNeuron_2];
        outputsHidden_1 = new float[countHiddenNeuron_1];
        outputsHidden_2 = new float[countHiddenNeuron_2];
        weightsOutputLayer = new float[countHiddenNeuron_2];
        weightedSumsHidden_1 = new float[countHiddenNeuron_1];
        weightedSumsHidden_2 = new float[countHiddenNeuron_2];

        outputs = new float[countOutputNeuron];

        lineRenderer.positionCount = savesos.Count;
        lineOutput.positionCount = savesos.Count;
        for (int i = 0; i < savesos.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(i, savesos[i].actionIndex));
        }

        //=============================
        thresoldOutputLayer = Random.Range(min, max);

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
            weightsOutputLayer[i] = Random.Range(min, max);
        }

        // ==========================================

        int iter = 0;
        while (teaching)
        {
            iter++;
            yield return null;

            print($"Итерация я ебу...{iter}");

            for (int iSample = 0; iSample < savesos.Count; iSample++)
            {
                for (int i = 0; i < countHiddenNeuron_1; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < p; j++)
                    {
                        float x = savesos[iSample].inputishe[j];
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
                float weightedSumOutputLayer = 0;
                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    float x = outputsHidden_2[i];
                    weightedSumOutputLayer += weightsOutputLayer[i] * x;
                }
                weightedSumOutputLayer -= thresoldOutputLayer;
                //float outputMain = SigmoidActivation(weightedSumOutputLayer);
                float outputMain = Sigmoda(weightedSumOutputLayer);//weightedSumOutputLayer;


                lineOutput.SetPosition(iSample, new Vector3(iSample, Mathf.Clamp(outputMain, -50, 50) * 10f));

                // Изминение порогов и весовых коэффициентов
                // Ебаный пиздец...

                //Вычисение ошибок для выходного слоя
                float errOutput = outputMain - savesos[iSample].actionIndex / 10f;
                // Вычисеие для второго скрытного слоя
                float[] errHiden_2 = new float[countHiddenNeuron_2];
                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    //errHiden[i] = errOutput * DerivativeReLU(weightedSumOutputLayer) * weightsOutputLayer[i];
                    errHiden_2[i] = errOutput * DerivativeSigmodos(weightedSumOutputLayer) * weightsOutputLayer[i];
                }
                // Вычисение для первого скрытно ёбыря
                float[] errHiden_1 = new float[countHiddenNeuron_1];
                for (int i = 0; i < countHiddenNeuron_1; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < countHiddenNeuron_2; j++)
                    {
                        errSum += errHiden_2[j] * DerivativeSigmodos(weightedSumsHidden_2[j]) * weightsHidden_2[j, i];
                    }
                    errHiden_1[i] = errSum;
                }

                
                //if(iter > 500)
                //{
                //    a = 0.0005f;
                //}

                // Корректировка весов от скрытого слоя к выходному i -> j
                //yield return null;
                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    float x = outputsHidden_2[i];
                    float w = weightsOutputLayer[i] - a * errOutput * x * DerivativeSigmodos(weightedSumOutputLayer);
                    weightsOutputLayer[i] = w;
                }
                thresoldOutputLayer = thresoldOutputLayer + a * errOutput * DerivativeSigmodos(weightedSumOutputLayer);

                // Корректировка весов от входного слоя к скрытому
                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    for (int j = 0; j < countHiddenNeuron_1; j++)
                    {
                        float w = weightsHidden_2[i, j];
                        float x = outputsHidden_1[j];
                        //w = w - a * errHiden[i] * DerivativeReLU(weightedSums[i]) * x;
                        w = w - a * errHiden_2[i] * DerivativeSigmodos(weightedSumsHidden_2[i]) * x;
                        weightsHidden_2[i, j] = w;
                    }
                    //thresoldsHidden[i] = thresoldsHidden[i] + a * errHiden[i] * DerivativeReLU(weightedSums[i]);
                    thresoldsHidden_2[i] = thresoldsHidden_2[i] + a * errHiden_2[i] * DerivativeSigmodos(weightedSumsHidden_2[i]);
                }

                for (int i = 0; i < countHiddenNeuron_1; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        float w = weightsHidden_1[i, j];
                        float x = savesos[iSample].inputishe[j];
                        //w = w - a * errHiden[i] * DerivativeReLU(weightedSums[i]) * x;
                        w = w - a * errHiden_1[i] * DerivativeSigmodos(weightedSumsHidden_1[i]) * x;
                        weightsHidden_1[i, j] = w;
                    }
                    //thresoldsHidden[i] = thresoldsHidden[i] + a * errHiden[i] * DerivativeReLU(weightedSums[i]);
                    thresoldsHidden_1[i] = thresoldsHidden_1[i] + a * errHiden_1[i] * DerivativeSigmodos(weightedSumsHidden_1[i]);
                }

            }
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
            float weightedSumOutputLayer = 0;
            for (int i = 0; i < countHiddenNeuron_2; i++)
            {
                float x = outputsHidden_2[i];
                weightedSumOutputLayer += weightsOutputLayer[i] * x;
            }
            weightedSumOutputLayer -= thresoldOutputLayer;
            
            float outputMain = Sigmoda(weightedSumOutputLayer) * 10f;

            int actionIndex = Mathf.RoundToInt(outputMain);

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
            return 0.01f;
    }

    float DerivativeReLU(float weightedSum)
    {
        if (weightedSum > 0)
            return 1;
        else
            return 0.01f;
    }
}