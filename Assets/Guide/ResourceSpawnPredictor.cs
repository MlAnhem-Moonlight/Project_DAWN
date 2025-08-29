using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
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

public class ResourceSpawnPredictor : MonoBehaviour
{
    [Header("Model Configuration")]
    [SerializeField] private LinearRegressionModel model;
    [SerializeField] private bool autoLoadAndTrain = true;
    [SerializeField] private float learningRate = 0.001f;
    [SerializeField] private int epochs = 1500;

    [Header("Training Data")]
    [SerializeField] private string trainingDataJsonPath = "Assets/Script/Environment/ingridient.json";
    [SerializeField] private List<TrainingData> trainingDataSet = new List<TrainingData>();

    [Header("Balance Factors")]
    [SerializeField] private float woodFactor = 1.0f;
    [SerializeField] private float stoneFactor = 1.0f;
    [SerializeField] private float ironFactor = 1.0f;
    [SerializeField] private float goldFactor = 1.0f;
    [SerializeField] private float meatFactor = 1.0f;

    [Header("Normalization Constants")]
    [SerializeField] private float maxLevel = 30.0f;
    [SerializeField] private float maxResourceValue = 1000.0f;

    [Header("Test Prediction")]
    [SerializeField] private int testLevel = 1;
    [SerializeField] private float testRemainingWood = 100f;
    [SerializeField] private float testRemainingStone = 80f;
    [SerializeField] private float testRemainingIron = 50f;
    [SerializeField] private float testRemainingGold = 30f;
    [SerializeField] private float testRemainingMeat = 60f;
    [SerializeField] private float testConsumedWood = 20f;
    [SerializeField] private float testConsumedStone = 15f;
    [SerializeField] private float testConsumedIron = 10f;
    [SerializeField] private float testConsumedGold = 5f;
    [SerializeField] private float testConsumedMeat = 12f;

    private ResourceDataGA resourceDataGA = new ResourceDataGA();

    // Input features: levelIndex(1) + remaining_resources(5) + consumed_resources(5) = 11
    private const int INPUT_FEATURES = 11;
    private const int OUTPUT_FEATURES = 5; // spawn_resources(5)

    void Start()
    {
        InitializeModel();

        if (autoLoadAndTrain)
        {
            LoadTrainingDataFromJson();
            if (trainingDataSet.Count > 0)
            {
                TrainModel();
            }
            else
            {
                Debug.LogWarning("No training data found! Model will use default predictions.");
            }
        }
    }

    private void InitializeModel()
    {
        model = new LinearRegressionModel(INPUT_FEATURES, OUTPUT_FEATURES);
        Debug.Log($"Model initialized with {INPUT_FEATURES} input features and {OUTPUT_FEATURES} output features.");
    }

    private void LoadTrainingDataFromJson()
    {
        trainingDataSet.Clear();

        if (!File.Exists(trainingDataJsonPath))
        {
            Debug.LogWarning($"Training data file not found: {trainingDataJsonPath}");
            CreateSampleTrainingData();
            return;
        }

        try
        {
            string json = File.ReadAllText(trainingDataJsonPath);
            var wrapper = JsonConvert.DeserializeObject<TrainingDataWrapper>(json);

            if (wrapper?.trainingData != null)
            {
                trainingDataSet.AddRange(wrapper.trainingData);
                Debug.Log($"Successfully loaded {trainingDataSet.Count} training samples from JSON.");
            }
            else
            {
                Debug.LogWarning("Invalid JSON format. Creating sample data.");
                CreateSampleTrainingData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading training data: {e.Message}");
            CreateSampleTrainingData();
        }
    }

    private void CreateSampleTrainingData()
    {
        Debug.Log("Creating sample training data...");

        trainingDataSet.Add(new TrainingData(
            1,
            new ResourceData(100, 80, 50, 30, 60),
            new ResourceData(20, 15, 10, 5, 12),
            new ResourceData(25, 20, 12, 8, 15)));

        trainingDataSet.Add(new TrainingData(
            2,
            new ResourceData(150, 120, 80, 50, 90),
            new ResourceData(30, 25, 15, 10, 18),
            new ResourceData(35, 30, 18, 12, 22)));

        trainingDataSet.Add(new TrainingData(
            3,
            new ResourceData(200, 160, 110, 70, 120),
            new ResourceData(40, 35, 20, 15, 25),
            new ResourceData(45, 40, 25, 18, 30)));
    }

    public void TrainModel()
    {
        if (trainingDataSet.Count == 0)
        {
            Debug.LogWarning("No training data available for training!");
            return;
        }

        List<float[]> inputs = new List<float[]>();
        List<float[]> outputs = new List<float[]>();

        foreach (var data in trainingDataSet)
        {
            inputs.Add(PrepareInput(data.levelIndex, data.remainingResources, data.consumedResources));
            // FIX: Normalize output data during training
            outputs.Add(NormalizeOutput(data.nextLevelSpawnResources.ToArray()));
        }

        Debug.Log($"Starting model training with {trainingDataSet.Count} samples...");
        model.Train(inputs, outputs, learningRate, epochs);
        Debug.Log("Model training completed successfully!");
    }

    private float[] PrepareInput(int levelIndex, ResourceData remaining, ResourceData consumed)
    {
        float[] input = new float[INPUT_FEATURES];
        float[] remainingArray = remaining.ToArray();
        float[] consumedArray = consumed.ToArray();

        // Normalize levelIndex
        input[0] = levelIndex / maxLevel;

        // Normalize remaining resources
        for (int i = 0; i < 5; i++)
        {
            input[i + 1] = remainingArray[i] / maxResourceValue;
        }

        // Normalize consumed resources
        for (int i = 0; i < 5; i++)
        {
            input[i + 6] = consumedArray[i] / maxResourceValue;
        }

        return input;
    }

    // FIX: Add method to normalize output data
    private float[] NormalizeOutput(float[] output)
    {
        float[] normalizedOutput = new float[output.Length];
        for (int i = 0; i < output.Length; i++)
        {
            normalizedOutput[i] = output[i] / maxResourceValue;
        }
        return normalizedOutput;
    }

    // FIX: Add method to denormalize output data
    private float[] DenormalizeOutput(float[] normalizedOutput)
    {
        float[] output = new float[normalizedOutput.Length];
        for (int i = 0; i < normalizedOutput.Length; i++)
        {
            output[i] = normalizedOutput[i] * maxResourceValue;
        }
        return output;
    }

    public ResourceData PredictNextLevelSpawn(int currentLevel, ResourceData remainingResources, ResourceData consumedResources)
    {
        // Dự đoán chính dựa trên lượng còn dư và tiêu hao hiện tại, sử dụng mô hình LinearRegressionModel
        if (!model.IsTrained)
        {
            Debug.LogWarning("Model is not trained yet! Using fallback prediction.");
            return GetFallbackPrediction(currentLevel, remainingResources, consumedResources);
        }

        try
        {
            float[] input = PrepareInput(currentLevel, remainingResources, consumedResources);
            float[] normalizedPrediction = model.Predict(input);
            float[] prediction = DenormalizeOutput(normalizedPrediction);

            // Làm tròn và áp dụng balance factor
            for (int i = 0; i < prediction.Length; i++)
            {
                prediction[i] = Mathf.Max(0, Mathf.Round(prediction[i]));
            }
            prediction[0] = Mathf.Round(prediction[0] * woodFactor);
            prediction[1] = Mathf.Round(prediction[1] * stoneFactor);
            prediction[2] = Mathf.Round(prediction[2] * ironFactor);
            prediction[3] = Mathf.Round(prediction[3] * goldFactor);
            prediction[4] = Mathf.Round(prediction[4] * meatFactor);

            var result = ResourceData.FromArray(prediction);
            Debug.Log($"[Predict] Model prediction for level {currentLevel + 1}: " +
                      $"Wood={result.wood}, Stone={result.stone}, Iron={result.iron}, Gold={result.gold}, Meat={result.meat}");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during prediction: {e.Message}");
            return GetFallbackPrediction(currentLevel, remainingResources, consumedResources);
        }
    }

    private ResourceData GetFallbackPrediction(int level, ResourceData remaining, ResourceData consumed)
    {
        float levelMultiplier = 1.0f + (level * 0.1f);

        return new ResourceData(
            (20f + consumed.wood * 0.5f) * levelMultiplier,
            (15f + consumed.stone * 0.5f) * levelMultiplier,
            (10f + consumed.iron * 0.5f) * levelMultiplier,
            (8f + consumed.gold * 0.5f) * levelMultiplier,
            (12f + consumed.meat * 0.5f) * levelMultiplier
        );
    }

    public void AddTrainingData(int levelIndex, ResourceData current, ResourceData consumed, ResourceData actualSpawn)
    {
        trainingDataSet.Add(new TrainingData(levelIndex, current.Clone(), consumed.Clone(), actualSpawn.Clone()));
        Debug.Log($"Added new training data for level {levelIndex}");
    }

    public void RetrainModel()
    {
        if (trainingDataSet.Count > 0)
        {
            TrainModel();
        }
        else
        {
            Debug.LogWarning("No training data available for retraining!");
        }
    }

    [ContextMenu("Test Prediction")]
    public void TestPrediction()
    {
        ResourceData remaining = new ResourceData(testRemainingWood, testRemainingStone, testRemainingIron, testRemainingGold, testRemainingMeat);
        ResourceData consumed = new ResourceData(testConsumedWood, testConsumedStone, testConsumedIron, testConsumedGold, testConsumedMeat);

        Debug.Log($"Testing prediction for level {testLevel}:");
        Debug.Log($"Remaining: Wood={remaining.wood}, Stone={remaining.stone}, Iron={remaining.iron}, Gold={remaining.gold}, Meat={remaining.meat}");
        Debug.Log($"Consumed: Wood={consumed.wood}, Stone={consumed.stone}, Iron={consumed.iron}, Gold={consumed.gold}, Meat={consumed.meat}");

        ResourceData prediction = PredictNextLevelSpawn(testLevel, remaining, consumed);
        Debug.Log($"Prediction for level {testLevel + 1}: Wood={prediction.wood:F1}, Stone={prediction.stone:F1}, Iron={prediction.iron:F1}, Gold={prediction.gold:F1}, Meat={prediction.meat:F1}");
    }

    public ResourceDataGA Prediction()
    {
        ResourceData remaining = new ResourceData(testRemainingWood, testRemainingStone, testRemainingIron, testRemainingGold, testRemainingMeat);
        ResourceData consumed = new ResourceData(testConsumedWood, testConsumedStone, testConsumedIron, testConsumedGold, testConsumedMeat);

        Debug.Log($"Testing prediction for level {testLevel}:");
        Debug.Log($"Remaining: Wood={remaining.wood}, Stone={remaining.stone}, Iron={remaining.iron}, Gold={remaining.gold}, Meat={remaining.meat}");
        Debug.Log($"Consumed: Wood={consumed.wood}, Stone={consumed.stone}, Iron={consumed.iron}, Gold={consumed.gold}, Meat={consumed.meat}");

        ResourceData prediction = PredictNextLevelSpawn(testLevel, remaining, consumed);
        resourceDataGA = new ResourceDataGA((int)prediction.wood, (int)prediction.stone, (int)prediction.iron, (int)prediction.gold, (int)prediction.meat);
        return resourceDataGA;
    }

    [ContextMenu("Save Sample Training Data")]
    public void SaveSampleTrainingData()
    {
        CreateSampleTrainingData();

        TrainingDataWrapper wrapper = new TrainingDataWrapper()
        {
            trainingData = trainingDataSet
        };

        string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
        File.WriteAllText(trainingDataJsonPath, json);

        Debug.Log($"Sample training data saved to {trainingDataJsonPath}");
    }
}