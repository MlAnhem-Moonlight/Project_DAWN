using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ResourceData
{
    public float wood;
    public float stone;
    public float iron;
    public float gold;
    public float meat;
    
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
        return new ResourceData(array[0], array[1], array[2], array[3], array[4]);
    }
}

[Serializable]
public class TrainingData
{
    public ResourceData currentResources;
    public ResourceData consumedResources;
    public ResourceData spawnedResources;
    
    public TrainingData(ResourceData current, ResourceData consumed, ResourceData spawned)
    {
        currentResources = current;
        consumedResources = consumed;
        spawnedResources = spawned;
    }
}

public class LinearRegressionModel
{
    private float[,] weights;
    private float[] bias;
    private int inputFeatures;
    private int outputFeatures;
    
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
        // Khởi tạo weights ngẫu nhiên nhỏ
        for (int i = 0; i < inputFeatures; i++)
        {
            for (int j = 0; j < outputFeatures; j++)
            {
                weights[i, j] = UnityEngine.Random.Range(-0.01f, 0.01f);
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
            throw new ArgumentException("Input size mismatch");
            
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
            
        int sampleCount = inputs.Count;
        
        for (int epoch = 0; epoch < epochs; epoch++)
        {
            float totalLoss = 0f;
            
            // Forward pass and calculate gradients
            float[,] weightGradients = new float[inputFeatures, outputFeatures];
            float[] biasGradients = new float[outputFeatures];
            
            for (int sample = 0; sample < sampleCount; sample++)
            {
                float[] prediction = Predict(inputs[sample]);
                
                // Calculate error
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
                        biasGradients[j] += error[j];
                    }
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
            
            // Log progress
            if (epoch % 100 == 0)
            {
                Debug.Log($"Epoch {epoch}: Loss = {totalLoss / sampleCount}");
            }
        }
    }
}

public class ResourceSpawnPredictor : MonoBehaviour
{
    [SerializeField] private LinearRegressionModel model;
    [SerializeField] private List<TrainingData> trainingDataSet = new List<TrainingData>();
    [SerializeField] private bool autoTrain = true;
    [SerializeField] private float learningRate = 0.001f;
    [SerializeField] private int epochs = 1000;

    [Header("JSON Training Data")]
    [SerializeField] private string trainingDataJsonPath = "Assets/Script/Environment/ingridient.json";

    [Header("Input prediction own")]
    public float ownWood;
    public float ownStone;
    public float ownIron;
    public float ownGold;
    public float ownMeat;

    [Header("Input prediction used")]
    public float usedWood;
    public float usedStone;
    public float usedIron;
    public float usedGold;
    public float usedMeat;

    [Header("Balance Factors")]
    [SerializeField] private float woodFactor = 1.0f;
    [SerializeField] private float stoneFactor = 1.0f;
    [SerializeField] private float ironFactor = 1.0f;
    [SerializeField] private float goldFactor = 1.0f;
    [SerializeField] private float meatFactor = 1.0f;

    private const int INPUT_FEATURES = 10;
    private const int OUTPUT_FEATURES = 5;

    void Start()
    {
        InitializeModel();

        LoadTrainingDataFromJson();

        if (autoTrain && trainingDataSet.Count > 0)
        {
            TrainModel();
        }
    }

    private void InitializeModel()
    {
        model = new LinearRegressionModel(INPUT_FEATURES, OUTPUT_FEATURES);
    }

    private void LoadTrainingDataFromJson()
    {
        trainingDataSet.Clear();
        if (!File.Exists(trainingDataJsonPath))
        {
            Debug.LogWarning($"Training data file not found: {trainingDataJsonPath}");
            return;
        }

        string json = File.ReadAllText(trainingDataJsonPath);
        TrainingDataWrapper wrapper = JsonUtility.FromJson<TrainingDataWrapper>(json);
        if (wrapper != null && wrapper.trainingData != null)
        {
            trainingDataSet.AddRange(wrapper.trainingData);
            Debug.Log($"Loaded {trainingDataSet.Count} training samples from JSON.");
        }
        else
        {
            Debug.LogWarning("Failed to parse training data from JSON.");
        }
    }

    [Serializable]
    private class TrainingDataWrapper
    {
        public List<TrainingData> trainingData;
    }

    public void TrainModel()
    {
        if (trainingDataSet.Count == 0)
        {
            Debug.LogWarning("No training data available!");
            return;
        }

        List<float[]> inputs = new List<float[]>();
        List<float[]> outputs = new List<float[]>();

        foreach (var data in trainingDataSet)
        {
            inputs.Add(PrepareInput(data.currentResources, data.consumedResources));
            outputs.Add(data.spawnedResources.ToArray());
        }

        Debug.Log($"Training model with {trainingDataSet.Count} samples...");
        model.Train(inputs, outputs, learningRate, epochs);
        Debug.Log("Model training completed!");
    }

    private float[] PrepareInput(ResourceData current, ResourceData consumed)
    {
        float[] input = new float[INPUT_FEATURES];
        float[] currentArray = current.ToArray();
        float[] consumedArray = consumed.ToArray();

        for (int i = 0; i < 5; i++)
        {
            input[i] = currentArray[i];
            input[i + 5] = consumedArray[i];
        }

        return input;
    }

    public ResourceData PredictSpawn(ResourceData currentResources, ResourceData consumedResources)
    {
        float[] input = PrepareInput(currentResources, consumedResources);
        float[] prediction = model.Predict(input);

        prediction[0] *= woodFactor;
        prediction[1] *= stoneFactor;
        prediction[2] *= ironFactor;
        prediction[3] *= goldFactor;
        prediction[4] *= meatFactor;

        for (int i = 0; i < prediction.Length; i++)
        {
            prediction[i] = Mathf.Max(0, prediction[i]);
        }

        return ResourceData.FromArray(prediction);
    }

    // Chạy dự đoán với input nhập từ bên ngoài
    public ResourceData RunPredictionFromInput(float wood, float stone, float iron, float gold, float meat,
                                               float consumedWood, float consumedStone, float consumedIron, float consumedGold, float consumedMeat)
    {
        ResourceData current = new ResourceData(wood, stone, iron, gold, meat);
        ResourceData consumed = new ResourceData(consumedWood, consumedStone, consumedIron, consumedGold, consumedMeat);
        ResourceData predicted = PredictSpawn(current, consumed);
        Debug.Log($"Input: Wood={wood}, Stone={stone}, Iron={iron}, Gold={gold}, Meat={meat} | Consumed: Wood={consumedWood}, Stone={consumedStone}, Iron={consumedIron}, Gold={consumedGold}, Meat={consumedMeat}");
        Debug.Log($"Predicted Spawn: Wood={predicted.wood:F1}, Stone={predicted.stone:F1}, Iron={predicted.iron:F1}, Gold={predicted.gold:F1}, Meat={predicted.meat:F1}");
        return predicted;
    }

    public void AddTrainingData(ResourceData current, ResourceData consumed, ResourceData actualSpawned)
    {
        trainingDataSet.Add(new TrainingData(current, consumed, actualSpawned));
    }

    public void RetrainModel()
    {
        TrainModel();
    }

    [ContextMenu("Test Prediction")]
    public void TestPrediction()
    {
        RunPredictionFromInput(ownWood, ownStone, ownIron, ownGold, ownMeat, usedWood, usedStone, usedIron, usedGold, usedMeat);
    }
}