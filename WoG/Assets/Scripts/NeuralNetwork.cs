using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public TrainingSample[] trainingSamples;
    public HiddenLayer[] hiddenLayers; // предполагается один элемент пока что
    
    int countInputNeurons;
    int countOutputNeurons;

    private IEnumerator Start()
    {
        yield return null;

        //Init();
    }

    private void Init()
    {
        countInputNeurons = trainingSamples[0].inputValues.Length;
        countOutputNeurons = trainingSamples[0].outputValues.Length;
    }

    public static float ActivationFunc(float weightedSum, ActivationFuncType funcType)
    {
        switch (funcType)
        {
            case ActivationFuncType.ReLU:
                return ReLUActivation(weightedSum);
            //case ActivationFuncType.Linear:
            //    break;
            //case ActivationFuncType.Sigmoid:
            //    break;
            default:
                return 0;
        }
    }

    public static float SigmoidActivation(float weightedSum)
    {
        var denominator = 1 + Mathf.Pow(Mathf.Epsilon, -weightedSum);
        var numerator = 1;
        return numerator / denominator;
    }

    public static float ReLUActivation(float weightedSum)
    {
        if (weightedSum > 0)
            return weightedSum;
        else
            return 0.01f;
    }
}

[System.Serializable]
public class Neuron
{
    public float[] weights;

    public float OutputValue { get; private set; }
    public float WeightedSum { get; private set; }

    float thresold;

    public ActivationFuncType activationFunc;

    public Neuron (int countLinks, Vector2 initializedRange)
    {
        weights = new float[countLinks];

        for (int i = 0; i < countLinks; i++)
        {
            weights[i] = Random.Range(initializedRange.x, initializedRange.y);
        }

        thresold = Random.Range(initializedRange.x, initializedRange.y);
    }


    public float GetOutputValue(float[] inputValues)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            float x = inputValues[i];
            float w = weights[i];
            WeightedSum += w * x;
        }
        WeightedSum -= thresold;
        OutputValue = NeuralNetwork.ActivationFunc(WeightedSum, activationFunc);
        return OutputValue;
    }


}

[System.Serializable]
public class TrainingSample
{
    public float[] inputValues;
    public float[] outputValues;
}

[System.Serializable]
public class HiddenLayer
{
    public int countNeurons;
    public ActivationFuncType activationFunc;
}

public enum ActivationFuncType
{
    ReLU,
    Linear,
    Sigmoid,
}