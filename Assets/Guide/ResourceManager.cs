using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceData
{
    public float wood { get; set; }
    public float stone { get; set; }
    public float iron { get; set; }
    public float gold { get; set; }
    public float meat { get; set; }

    public ResourceData() { }

    public ResourceData(float wood, float stone, float iron, float gold, float meat)
    {
        this.wood = wood;
        this.stone = stone;
        this.iron = iron;
        this.gold = gold;
        this.meat = meat;
    }

    public float[] ToArray()
    {
        return new float[] { wood, stone, iron, gold, meat };
    }

    public static ResourceData FromArray(float[] array)
    {
        if (array.Length < 5)
            throw new ArgumentException("Array must have at least 5 elements");
        return new ResourceData(array[0], array[1], array[2], array[3], array[4]);
    }

    public ResourceData Clone()
    {
        return new ResourceData(wood, stone, iron, gold, meat);
    }
}

[Serializable]
public class TrainingData
{
    public int levelIndex { get; set; }
    public ResourceData remainingResources { get; set; }
    public ResourceData consumedResources { get; set; }
    public ResourceData nextLevelSpawnResources { get; set; }

    public TrainingData() { }

    public TrainingData(int levelIndex, ResourceData current, ResourceData consumed, ResourceData spawned)
    {
        this.levelIndex = levelIndex;
        remainingResources = current;
        consumedResources = consumed;
        nextLevelSpawnResources = spawned;
    }
}

[Serializable]
public class TrainingDataWrapper
{
    public List<TrainingData> trainingData;
}

public class LinearRegressionModel
{
    private float[,] weights;
    private float[] bias;
    private int inputFeatures;
    private int outputFeatures;
    private bool isTrained = false;

    public bool IsTrained => isTrained;

    public LinearRegressionModel(int inputFeatures, int outputFeatures)
    {
        this.inputFeatures = inputFeatures;
        this.outputFeatures = outputFeatures;
        this.weights = new float[inputFeatures, outputFeatures];
        this.bias = new float[outputFeatures];
        InitializeWeights();
    }

    private void InitializeWeights()
    {
        System.Random random = new System.Random();
        for (int i = 0; i < inputFeatures; i++)
        {
            for (int j = 0; j < outputFeatures; j++)
            {
                weights[i, j] = (float)(random.NextDouble() - 0.5) * 0.02f;
            }
        }

        for (int j = 0; j < outputFeatures; j++)
        {
            bias[j] = 0f;
        }
    }

    public float[] Predict(float[] input)
    {
        if (input.Length != inputFeatures)
            throw new ArgumentException($"Input size mismatch. Expected {inputFeatures}, got {input.Length}");

        float[] output = new float[outputFeatures];
        for (int j = 0; j < outputFeatures; j++)
        {
            output[j] = bias[j];
            for (int i = 0; i < inputFeatures; i++)
            {
                output[j] += input[i] * weights[i, j];
            }
        }

        return output;
    }

    public void Train(List<float[]> inputs, List<float[]> outputs, float learningRate = 0.001f, int epochs = 1000)
    {
        if (inputs.Count != outputs.Count)
            throw new ArgumentException("Input and output count mismatch");

        if (inputs.Count == 0)
        {
            Debug.LogWarning("No training data provided");
            return;
        }

        int sampleCount = inputs.Count;
        float bestLoss = float.MaxValue;

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            float totalLoss = 0f;

            // Forward pass and calculate gradients
            float[,] weightGradients = new float[inputFeatures, outputFeatures];
            float[] biasGradients = new float[outputFeatures];

            for (int sample = 0; sample < sampleCount; sample++)
            {
                float[] prediction = Predict(inputs[sample]);

                // Calculate error and loss
                float[] error = new float[outputFeatures];
                for (int j = 0; j < outputFeatures; j++)
                {
                    error[j] = prediction[j] - outputs[sample][j];
                    totalLoss += error[j] * error[j];
                }

                // Accumulate gradients
                for (int i = 0; i < inputFeatures; i++)
                {
                    for (int j = 0; j < outputFeatures; j++)
                    {
                        weightGradients[i, j] += error[j] * inputs[sample][i];
                    }
                }

                for (int j = 0; j < outputFeatures; j++)
                {
                    biasGradients[j] += error[j];
                }
            }

            // Update weights and bias
            for (int i = 0; i < inputFeatures; i++)
            {
                for (int j = 0; j < outputFeatures; j++)
                {
                    weights[i, j] -= learningRate * weightGradients[i, j] / sampleCount;
                }
            }

            for (int j = 0; j < outputFeatures; j++)
            {
                bias[j] -= learningRate * biasGradients[j] / sampleCount;
            }

            totalLoss /= sampleCount;
            if (totalLoss < bestLoss)
            {
                bestLoss = totalLoss;
            }

            // Log progress
            if (epoch % 200 == 0 || epoch == epochs - 1)
            {
                Debug.Log($"Training Epoch {epoch}/{epochs}: Loss = {totalLoss:F6}");
            }
        }

        isTrained = true;
        Debug.Log($"Training completed! Best loss: {bestLoss:F6}");
    }
}