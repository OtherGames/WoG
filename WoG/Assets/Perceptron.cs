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

    List<float> etalons = new List<float>();
    [SerializeField]
    List<float> weights = new List<float>();
    [SerializeField]
    float[] yValues;
    float threshold;

    [SerializeField]
    List<float> weightsHiddenLayer_1 = new List<float>();
    [SerializeField]
    List<float> weightsHiddenLayer_2 = new List<float>();
    [SerializeField]
    List<float> weightsHiddenLayer_3 = new List<float>();
    [SerializeField]
    List<float> weightsHiddenLayer_4 = new List<float>();
    [SerializeField]
    List<float> weightsHiddenLayer_5 = new List<float>();
    [SerializeField]
    List<float> weightsOutputLayer = new List<float>();
    float thresoldHiddenLayer_1;
    float thresoldHiddenLayer_2;
    float thresoldHiddenLayer_3;
    float thresoldHiddenLayer_4;
    float thresoldHiddenLayer_5;
    float thresoldOutputLayer;
    List<float> outputsHiddenLayer = new List<float>();

    private IEnumerator Start()
    {
        var capacity = rowLength - p;
        yValues = new float[capacity];

        var pos = predictLine.transform.position;
        pos.x = lineRenderer.transform.position.x + p;
        predictLine.transform.position = pos;

        for (int i = 0; i < rowLength; i++)
        {
            yield return null;

            float x = i;
            float y = 1.8f * Mathf.Sin(1.3f * i);// + 0.5f;
            lineRenderer.positionCount = i + 1;
            lineRenderer.SetPosition(i, new Vector2(x, y));

            etalons.Add(y);
        }

        //etalons.Add(-1.153f);
        //etalons.Add(-1.153f);
        //etalons.Add(-1.053f);
        //etalons.Add(-1.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(1.123f);
        //etalons.Add(-1.153f);
        //etalons.Add(-1.053f);
        //etalons.Add(-1.153f);
        //etalons.Add(1.153f);
        //etalons.Add(0.153f);
        //etalons.Add(-1.153f);
        //etalons.Add(-1.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(0.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(-1.113f);
        //etalons.Add(-0.153f);
        //etalons.Add(1.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(-0.133f);
        //etalons.Add(-0.153f);
        //etalons.Add(-0.143f);
        //etalons.Add(1.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(-0.153f);
        //etalons.Add(-0.583f);
        //etalons.Add(-1.153f);
        //etalons.Add(-0.153f);

        //for (int i = 0; i < rowLength; i++)
        //{
        //    yield return null;
        //    lineRenderer.positionCount = i + 1;
        //    lineRenderer.SetPosition(i, new Vector2(i, etalons[i]));
        //}

        float min = -0.8f;
        float max = 0.8f;
        // инициализация весов
        for (int i = 0; i < p; i++)
        {
            weightsHiddenLayer_1.Add(Random.Range(min, max));
            weightsHiddenLayer_2.Add(Random.Range(min, max));
            weightsHiddenLayer_3.Add(Random.Range(min, max));
            weightsHiddenLayer_4.Add(Random.Range(min, max));
            weightsHiddenLayer_5.Add(Random.Range(min, max));
        }

        for (int i = 0; i < 5; i++)
        {
            weightsOutputLayer.Add(Random.Range(min, max));
        }

        thresoldHiddenLayer_1 = Random.Range(min, max);
        thresoldHiddenLayer_2 = Random.Range(min, max);
        thresoldHiddenLayer_3 = Random.Range(min, max);
        thresoldHiddenLayer_4 = Random.Range(min, max);
        thresoldHiddenLayer_5 = Random.Range(min, max);
        thresoldOutputLayer = Random.Range(min, max);


        for (int iSample = 0; iSample < rowLength - p; iSample++)
        {
            yield return null;
            // Подaча входных образов
            float weightedSumHiddelLayer = 0;
            // Вычисление выходных значений 1-го нейрона скрытого слоя
            for (int i = 0; i < p; i++)
            {
                float x = etalons[i + iSample];
                weightedSumHiddelLayer += weightsHiddenLayer_1[i] * x;
            }
            weightedSumHiddelLayer -= thresoldHiddenLayer_1;

            float output_1 = SigmoidActivation(weightedSumHiddelLayer);
            
            if(outputsHiddenLayer.Count < 5)
                outputsHiddenLayer.Add(output_1);
            else
                outputsHiddenLayer[0] = output_1;

            yield return null;
            // Вычисление выходных значений 2-го нейрона скрытого слоя
            weightedSumHiddelLayer = 0;
            for (int i = 0; i < p; i++)
            {
                float x = etalons[i + iSample];
                weightedSumHiddelLayer += weightsHiddenLayer_2[i] * x;
            }
            weightedSumHiddelLayer -= thresoldHiddenLayer_2;

            float output_2 = SigmoidActivation(weightedSumHiddelLayer);
            if (outputsHiddenLayer.Count < 5)
                outputsHiddenLayer.Add(output_2);
            else
                outputsHiddenLayer[1] = output_2;

            yield return null;
            // Вычисление выходных значений 3-го нейрона скрытого слоя
            weightedSumHiddelLayer = 0;
            for (int i = 0; i < p; i++)
            {
                float x = etalons[i + iSample];
                weightedSumHiddelLayer += weightsHiddenLayer_3[i] * x;
            }
            weightedSumHiddelLayer -= thresoldHiddenLayer_3;

            float output_3 = SigmoidActivation(weightedSumHiddelLayer);
            if (outputsHiddenLayer.Count < 5)
                outputsHiddenLayer.Add(output_3);
            else
                outputsHiddenLayer[2] = output_3;

            yield return null;
            // Вычисление выходных значений 4-го нейрона скрытого слоя
            weightedSumHiddelLayer = 0;
            for (int i = 0; i < p; i++)
            {
                float x = etalons[i + iSample];
                weightedSumHiddelLayer += weightsHiddenLayer_4[i] * x;
            }
            weightedSumHiddelLayer -= thresoldHiddenLayer_4;

            float output_4 = SigmoidActivation(weightedSumHiddelLayer);
            if (outputsHiddenLayer.Count < 5)
                outputsHiddenLayer.Add(output_4);
            else
                outputsHiddenLayer[3] = output_4;

            yield return null;
            // Вычисление выходных значений 5-го нейрона скрытого слоя
            weightedSumHiddelLayer = 0;
            for (int i = 0; i < p; i++)
            {
                float x = etalons[i + iSample];
                weightedSumHiddelLayer += weightsHiddenLayer_5[i] * x;
            }
            weightedSumHiddelLayer -= thresoldHiddenLayer_5;

            float output_5 = SigmoidActivation(weightedSumHiddelLayer);
            if (outputsHiddenLayer.Count < 5)
                outputsHiddenLayer.Add(output_5);
            else
                outputsHiddenLayer[4] = output_5;


            yield return null;
            // Вычисление выходных значений выходного слоя
            float weightedSumOutputLayer = 0;
            for (int i = 0; i < outputsHiddenLayer.Count; i++)
            {
                float x = outputsHiddenLayer[i];
                weightedSumOutputLayer += weightsOutputLayer[i] * x;
            }
            weightedSumOutputLayer -= thresoldOutputLayer;
            float outputMain = weightedSumOutputLayer;//SigmoidActivation(weightedSumOutputLayer);



            yValues[iSample] = outputMain;
            if (predictLine.positionCount < rowLength - p)
                predictLine.positionCount = iSample + 1;
            predictLine.SetPosition(iSample, new Vector3(iSample, outputMain));

            // Изминение порогов и весовых коэффициентов
            // Ебаный пиздец...


        }
    }

    float SigmoidActivation(float weightedSum)
    {
        var denominator = 1 + Mathf.Pow(Mathf.Epsilon, -weightedSum);
        var numerator = 1;
        return numerator / denominator;
        //return weightedSum;
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
