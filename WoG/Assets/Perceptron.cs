using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Perceptron : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;
    [SerializeField]
    LineRenderer predictLine;

    [SerializeField]
    private int rowLength = 30;

    [SerializeField]
    private int p = 8;
    [SerializeField]
    private int countHiddenNeuron = 5;
    [SerializeField]
    float ebala = 0.37f;
    [SerializeField]
    float min = -0.5f;
    [SerializeField]
    float max = 0.5f;

    List<float> etalons = new List<float>();
    [SerializeField]
    List<float> weights = new List<float>();
    [SerializeField]
    float[] yValues;
    float threshold;

    float[,] weightsHidden;
    float[] thresoldsHidden;
    float[] weightedSums;
    float[] outputsHidden;
    
    [SerializeField]
    float[] weightsOutputLayer;
    float thresoldOutputLayer;


    private IEnumerator Start()
    {
        weightsHidden = new float[countHiddenNeuron, p];
        thresoldsHidden = new float[countHiddenNeuron];
        outputsHidden = new float[countHiddenNeuron];
        weightsOutputLayer = new float[countHiddenNeuron];
        weightedSums = new float[countHiddenNeuron];

        var capacity = rowLength - p;
        yValues = new float[capacity];

        var pos = predictLine.transform.position;
        pos.x = lineRenderer.transform.position.x + p;
        predictLine.transform.position = pos;

        //for (int i = 0; i < rowLength; i++)
        //{
        //    yield return null;

        //    float x = i;
        //    //float y = 1.8f * Mathf.Sin(1.3f * i);// + 0.5f;
        //    float y = .8f * Mathf.Sin(1.3f * i);// + 0.5f;
        //    //y = Mathf.Clamp01(y);
        //    lineRenderer.positionCount = i + 1;
        //    lineRenderer.SetPosition(i, new Vector2(x, y));

        //    etalons.Add(y);
        //}

        etalons.Add(-1.153f);
        etalons.Add(-1.153f);
        etalons.Add(-1.053f);
        etalons.Add(-1.153f);
        etalons.Add(-0.153f);
        etalons.Add(-0.153f);
        etalons.Add(1.123f);
        etalons.Add(-1.153f);
        etalons.Add(-1.053f);
        etalons.Add(-1.153f);
        etalons.Add(1.153f);
        etalons.Add(0.153f);
        etalons.Add(-1.153f);
        etalons.Add(-1.153f);
        etalons.Add(-0.153f);
        etalons.Add(0.153f);
        etalons.Add(-0.153f);
        etalons.Add(-1.113f);
        etalons.Add(-0.153f);
        etalons.Add(1.153f);
        etalons.Add(-0.153f);
        etalons.Add(-0.133f);
        etalons.Add(-0.153f);
        etalons.Add(-0.143f);
        etalons.Add(1.153f);
        etalons.Add(-0.153f);
        etalons.Add(-0.153f);
        etalons.Add(-0.583f);
        etalons.Add(-1.153f);
        etalons.Add(-0.153f);
        etalons.Add(-0.583f);
        etalons.Add(-0.353f);
        etalons.Add(-0.153f);

        for (int i = 0; i < rowLength; i++)
        {
            yield return null;
            lineRenderer.positionCount = i + 1;
            lineRenderer.SetPosition(i, new Vector2(i, etalons[i]));
        }



        // инициализация весов
        thresoldOutputLayer = Random.Range(min, max);

        for (int i = 0; i < countHiddenNeuron; i++)
        {
            for (int j = 0; j < p; j++)
            {
                weightsHidden[i, j] = Random.Range(min, max);
            }

            thresoldsHidden[i] = Random.Range(min, max);
            weightsOutputLayer[i] = Random.Range(min, max);
        }


        // Подaча входных образов
        for (int pisdat = 0; pisdat < 10000; pisdat++)
        {
            yield return null;
            print($"Итерация я ебу...{pisdat}");

            for (int iSample = 0; iSample < rowLength - p; iSample++)
            {

                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    float wSum = 0;
                    for (int j = 0; j < p; j++)
                    {
                        float x = etalons[j + iSample];
                        wSum += weightsHidden[i, j] * x;
                    }
                    wSum -= thresoldsHidden[i];
                    weightedSums[i] = wSum;
                    outputsHidden[i] = SigmoidActivation(wSum);
                }


                // Вычисление выходных значений 1-го нейрона скрытого слоя
                //for (int i = 0; i < p; i++)
                //{
                //    float x = etalons[i + iSample];
                //    weightedSumHiddelLayer += weightsHiddenLayer_1[i] * x;
                //}
                //weightedSumHiddelLayer -= thresoldHiddenLayer_1;

                //float output_1 = SigmoidActivation(weightedSumHiddelLayer);

                //if (outputsHiddenLayer.Count < 5)
                //    outputsHiddenLayer.Add(output_1);
                //else
                //    outputsHiddenLayer[0] = output_1;




                //yield return null;
                // Вычисление выходных значений выходного слоя
                float weightedSumOutputLayer = 0;
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    float x = outputsHidden[i];
                    weightedSumOutputLayer += weightsOutputLayer[i] * x;
                }
                weightedSumOutputLayer -= thresoldOutputLayer;
                //float outputMain = SigmoidActivation(weightedSumOutputLayer);
                float outputMain = weightedSumOutputLayer;


                yValues[iSample] = outputMain;
                if (predictLine.positionCount < rowLength - p)
                    predictLine.positionCount = iSample + 1;
                predictLine.SetPosition(iSample, new Vector3(iSample, outputMain));




                // Изминение порогов и весовых коэффициентов
                // Ебаный пиздец...

                //Вычисение ошибок для выходного слоя и скрытых слоев
                float errOutput = outputMain - etalons[iSample + p];

                float[] errHiden = new float[countHiddenNeuron];
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    errHiden[i] = errOutput * DerivativeSigmoid(weightedSumOutputLayer) * weightsOutputLayer[i];
                }

                float a = 0.05f;

                // Корректировка весов от скрытого слоя к выходному i -> j
                //yield return null;
                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    float w = weightsOutputLayer[i] - a * errOutput * DerivativeSigmoid(weightedSumOutputLayer) * outputsHidden[i];
                    weightsOutputLayer[i] = w;
                }

                thresoldOutputLayer = thresoldOutputLayer + a * errOutput * DerivativeSigmoid(weightedSumOutputLayer);

                // Корректировка весов от входного слоя к скрытому

                for (int i = 0; i < countHiddenNeuron; i++)
                {
                    for (int j = 0; j < p; j++)
                    {
                        float w = weightsHidden[i, j];
                        float x = etalons[j + iSample];
                        w = w - a * errHiden[i] * DerivativeSigmoid(weightedSums[i]) * x;
                        weightsHidden[i, j] = w;
                    }
                    thresoldsHidden[i] = thresoldsHidden[i] + a * errHiden[i] * DerivativeSigmoid(weightedSums[i]);
                }

            }
        }
    }

    float SigmoidActivation(float weightedSum)
    {
        //var denominator = 1 + Mathf.Pow(Mathf.Epsilon, -weightedSum);
        //var numerator = 1;
        //return numerator / denominator;

        if (weightedSum > 0)
            return weightedSum;
        else
            return 0.01f;
        
    }

    float DerivativeSigmoid(float weightedSum)
    {
        //return SigmoidActivation(weightedSum) * (1 - SigmoidActivation(weightedSum));
        if (weightedSum > 0)
            return 1;
        else
            return 0.01f;
    }

    IEnumerator OneLayerPerchick()
    {
        // инициализация весов
        for (int i = 0; i < p; i++)
        {
            weights.Add(Random.Range(0f, 1f));
            threshold = Random.Range(0f, 1f);
        }

        for (int pisda = 0; pisda < 500; pisda++)
        {
            yield return null;
            print($"Намбер итерасыи {pisda}");
            // Вычисление выходных значений сети
            for (int iSample = 0; iSample < rowLength - p; iSample++)
            {
                float weightedSum = 0;

                for (int i = 0; i < p; i++)
                {
                    float xValue = etalons[i + iSample];
                    weightedSum += weights[i] * xValue;
                    //yield return null;
                }

                yield return null;
                weightedSum -= threshold;

                yValues[iSample] = weightedSum;
                if (predictLine.positionCount < rowLength - p)
                    predictLine.positionCount = iSample + 1;
                predictLine.SetPosition(iSample, new Vector3(iSample, weightedSum));

                // Изминение порогов и весовых коэффициентов

                for (int i = 0; i < p; i++)
                {
                    float xValue = etalons[i + iSample];
                    var nextW = weights[i] - 0.01f * (yValues[iSample] - etalons[iSample + p]) * xValue;
                    weights[i] = nextW;

                    //yield return null;
                }

                threshold = threshold + 0.01f * (yValues[iSample] - etalons[iSample + p]);

                yield return null;
            }


        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }
}
