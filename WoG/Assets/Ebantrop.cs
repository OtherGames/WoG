using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ebantrop : MonoBehaviour
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
    float[] etalons;

    [SerializeField]
    float[] outputs;
    //float threshold;

    float[,] weightsHidden_1;
    float[,] weightsHidden_2;
    float[,] weightsOutputLayer;
   
    float[] weightedSumsHidden_1;
    float[] weightedSumsHidden_2;
    float[] weightedSumsOutput;

    float[] outputsHidden_1;
    float[] outputsHidden_2;


    [SerializeField]
    float[] thresoldsHidden_1;
    [SerializeField]
    float[] thresoldsHidden_2;
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

        countHiddenNeuron_1 = 500;//[0].inputishe.Count;// / 2;
        countHiddenNeuron_2 = 500;
        p = savesos[0].inputishe.Count;

        weightsHidden_1 = new float[countHiddenNeuron_1, p];
        weightsHidden_2 = new float[countHiddenNeuron_2, countHiddenNeuron_1];
        thresoldsHidden_1 = new float[countHiddenNeuron_1];
        thresoldsHidden_2 = new float[countHiddenNeuron_2];
        thresoldsOutputLayer = new float[countOutputNeuron];
        outputsHidden_1 = new float[countHiddenNeuron_1];
        outputsHidden_2 = new float[countHiddenNeuron_2];
        weightsOutputLayer = new float[countOutputNeuron, countHiddenNeuron_2];
        weightedSumsHidden_1 = new float[countHiddenNeuron_1];
        weightedSumsHidden_2 = new float[countHiddenNeuron_2];
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
            for (int j = 0; j < countHiddenNeuron_2; j++)
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

                //yield return null;
                // Изминение порогов и весовых коэффициентов
                // Ебаный пиздец...
                int actIdx = (int)savesos[iSample].actionIndex;
                
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    if(i == actIdx)
                    {
                        etalons[i] = 0.98f;
                    }
                    else
                    {
                        etalons[i] = Mathf.Clamp(outputs[i], 0f, 0.7f);
                    }
                }
                //Вычисение ошибок для выходного слоя
                float[] errsOutput = new float[countOutputNeuron];
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    errsOutput[i] = outputs[i] - etalons[i];
                    //if (i == actIdx)
                    //{
                    //    if(etalons[i] < outputs[i])
                    //    {
                    //        errsOutput[i] = 0.0000001f;
                    //    }
                    //}
                    //else
                    //{
                        
                    //    //if (outputs[actIdx] > outputs[i])
                    //    //{
                    //    //    errsOutput[i] = 0;
                    //    //}
                    //    //else
                    //    //{
                    //    //    errsOutput[i] = outputs[i] - etalons[i];
                    //    //}
                    //}
                }
                // Вычисеие для второго скрытного слоя
                float[] errHiden_2 = new float[countHiddenNeuron_2];
                for (int i = 0; i < countHiddenNeuron_2; i++)
                {
                    float errSum = 0;
                    for (int j = 0; j < countOutputNeuron; j++)
                    {
                        //errHiden[i] = errOutput * DerivativeReLU(weightedSumOutputLayer) * weightsOutputLayer[i];
                        errSum += errsOutput[j] * DerivativeSigmodos(weightedSumsOutput[j]) * weightsOutputLayer[j, i];
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
                for (int i = 0; i < countOutputNeuron; i++)
                {
                    for (int j = 0; j < countHiddenNeuron_2; j++)
                    {
                        float x = outputsHidden_2[j];
                        float w = weightsOutputLayer[i,j] - a * errsOutput[i] * x * DerivativeSigmodos(weightedSumsOutput[i]);
                        weightsOutputLayer[i,j] = w;
                    }
                    thresoldsOutputLayer[i] = thresoldsOutputLayer[i] + a * errsOutput[i] * DerivativeSigmodos(weightedSumsOutput[i]);
                }

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

                yield return null;

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

            print($"Ошибкос {MEGA_ERROR} а вот хуяция - {iter}");
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
            return 0.01f * weightedSum;
    }

    float DerivativeReLU(float weightedSum)
    {
        if (weightedSum > 0)
            return 1;
        else
            return 0.01f;
    }
}