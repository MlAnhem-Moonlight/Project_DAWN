using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class ResourceDataGA
{
    public int wood = 0;
    public int stone = 0;
    public int iron = 0;
    public int gold = 0;
    public int meat = 0;

    public ResourceDataGA() { }

    public ResourceDataGA(int w, int s, int i, int g, int m)
    {
        wood = w; stone = s; iron = i; gold = g; meat = m;
    }

    public ResourceDataGA Clone()
    {
        return new ResourceDataGA(wood, stone, iron, gold, meat);
    }

    public int Total => wood + stone + iron + gold + meat;
}

[System.Serializable]
public class GameObjectAllocation
{
    public string objectName;
    public ResourceDataGA resourceCost;
    public int quantity;

    public GameObjectAllocation(string name, ResourceDataGA cost)
    {
        objectName = name;
        resourceCost = cost;
        quantity = 0;
    }

    public ResourceDataGA GetTotalCost()
    {
        return new ResourceDataGA(
            resourceCost.wood * quantity,
            resourceCost.stone * quantity,
            resourceCost.iron * quantity,
            resourceCost.gold * quantity,
            resourceCost.meat * quantity
        );
    }
}

[System.Serializable]
public class Individual
{
    public List<GameObjectAllocation> allocations;
    public float fitness;

    public Individual(List<GameObjectAllocation> template)
    {
        allocations = new List<GameObjectAllocation>();
        foreach (var allocation in template)
        {
            allocations.Add(new GameObjectAllocation(allocation.objectName, allocation.resourceCost));
        }
        fitness = 0f;
    }

    public ResourceDataGA GetTotalUsedResources()
    {
        var total = new ResourceDataGA();
        foreach (var allocation in allocations)
        {
            var cost = allocation.GetTotalCost();
            total.wood += cost.wood;
            total.stone += cost.stone;
            total.iron += cost.iron;
            total.gold += cost.gold;
            total.meat += cost.meat;
        }
        return total;
    }

    public Individual Clone()
    {
        var clone = new Individual(allocations);
        for (int i = 0; i < allocations.Count; i++)
        {
            clone.allocations[i].quantity = allocations[i].quantity;
        }
        clone.fitness = fitness;
        return clone;
    }
}

public class ResourceAllocationGA : MonoBehaviour
{
    private ResourceSpawnPredictor predictor;
    [Header("Available Resources")]
    public ResourceDataGA availableResources;

    [Header("GA Parameters")]
    public int populationSize = 50;
    public int generations = 100;
    public float mutationRate = 0.5f;
    public float crossoverRate = 0.8f;
    public int eliteCount = 5;

    [Header("Resource Priority (1=highest, 5=lowest)")]
    public bool usePriorityAllocation = true;
    public int meatPriority = 1;
    public int goldPriority = 2;
    public int ironPriority = 3;
    public int stonePriority = 4;
    public int woodPriority = 5;

    [Header("Results")]
    public Individual bestSolution;
    public bool isOptimized = false;

    [Header("Ecosystem Balance Parameters")]
    [SerializeField] private float baseHuntTime = 80f; // 1 minute 20 seconds in seconds
    [SerializeField] private float huntTimeReductionPerWolf = 10f; // Time reduction per additional wolf
    [SerializeField] private float minHuntTime = 20f; // Minimum time regardless of wolf count

    private List<GameObjectAllocation> objectTemplate;
    private List<Individual> population;
    private System.Random random;

    // Add this struct to track ecosystem state
    private struct EcosystemState
    {
        public int totalWolves;
        public int totalDeer;
        public float currentHuntTime;
        public float huntTimer;
        public bool isHunting;
    }
    private EcosystemState ecosystemState;

    void Awake()
    {
        predictor = GetComponent<ResourceSpawnPredictor>();
    }

    void Start()
    {
        // Wait until predictor is ready and has run its initialization
        StartCoroutine(WaitForPredictionAndStart());
    }

    IEnumerator WaitForPredictionAndStart()
    {
        // Wait for one frame to ensure predictor's Start has run
        yield return null;

        // Now get the prediction and initialize GA
        availableResources = predictor != null ? predictor.Prediction() : new ResourceDataGA(300, 250, 100, 180, 350);
        random = new System.Random();
        InitializeObjectTemplate();
        RunGeneticAlgorithm();
    }

    void InitializeObjectTemplate()
    {
        objectTemplate = new List<GameObjectAllocation>();

        // Load data từ JSON thông qua Ingredient class
        var envData = Ingredient.GetEnvironmentData();

        if (envData != null && envData.objects != null)
        {
            foreach (var obj in envData.objects)
            {
                // Chuyển đổi từ IngredientEntry[] thành ResourceDataGA
                var resourceCost = new ResourceDataGA();

                foreach (var ingredient in obj.ingredients)
                {
                    switch (ingredient.type.ToLower())
                    {
                        case "wood":
                            resourceCost.wood = ingredient.quantity;
                            break;
                        case "stone":
                            resourceCost.stone = ingredient.quantity;
                            break;
                        case "iron":
                            resourceCost.iron = ingredient.quantity;
                            break;
                        case "gold":
                            resourceCost.gold = ingredient.quantity;
                            break;
                        case "meat":
                            resourceCost.meat = ingredient.quantity;
                            break;
                    }
                }

                objectTemplate.Add(new GameObjectAllocation(obj.id, resourceCost));
            }

            Debug.Log($"Đã load {objectTemplate.Count} object templates từ JSON");
        }
        else
        {
            Debug.LogError("Không thể load environment data từ JSON, sử dụng default template");
            // Fallback to default template nếu không load được từ JSON
            LoadDefaultTemplate();
        }
    }

    void RunGeneticAlgorithm()
    {
        Debug.Log("Starting Genetic Algorithm for Resource Allocation...");

        // Initialize population
        InitializePopulation();

        for (int generation = 0; generation < generations; generation++)
        {
            // Evaluate fitness
            EvaluateFitness();

            // Sort by fitness
            population = population.OrderByDescending(ind => ind.fitness).ToList();

            if (generation % 20 == 0)
            {
                Debug.Log($"Generation {generation}: Best Fitness = {population[0].fitness:F2}");
            }

            // Create new generation
            var newPopulation = new List<Individual>();

            // Keep elite individuals
            for (int i = 0; i < eliteCount && i < population.Count; i++)
            {
                newPopulation.Add(population[i].Clone());
            }

            // Generate offspring
            while (newPopulation.Count < populationSize)
            {
                var parent1 = SelectParent();
                var parent2 = SelectParent();

                var children = Crossover(parent1, parent2);

                Mutate(children.Item1);
                Mutate(children.Item2);

                newPopulation.Add(children.Item1);
                if (newPopulation.Count < populationSize)
                    newPopulation.Add(children.Item2);
            }

            population = newPopulation;
        }

        // Final evaluation and select best
        EvaluateFitness();
        bestSolution = population.OrderByDescending(ind => ind.fitness).First();
        isOptimized = true;

        DisplayResults();
    }

    void InitializePopulation()
    {
        population = new List<Individual>();

        for (int i = 0; i < populationSize; i++)
        {
            var individual = new Individual(objectTemplate);
            RandomizeAllocation(individual);
            population.Add(individual);
        }
    }

    void RandomizeAllocation(Individual individual)
    {
        if (usePriorityAllocation)
        {
            RandomizeWithPriority(individual);
        }
        else
        {
            // Original random allocation
            var remaining = availableResources.Clone();

            foreach (var allocation in individual.allocations)
            {
                int maxQuantity = CalculateMaxQuantity(allocation.resourceCost, remaining);

                if (maxQuantity > 0)
                {
                    int quantity = random.Next(0, Mathf.Max(1, (int)(maxQuantity * 0.9f)));
                    allocation.quantity = quantity;

                    var used = allocation.GetTotalCost();
                    remaining.wood -= used.wood;
                    remaining.stone -= used.stone;
                    remaining.iron -= used.iron;
                    remaining.gold -= used.gold;
                    remaining.meat -= used.meat;
                }
            }
        }
    }

    void RandomizeWithPriority(Individual individual)
    {
        var remaining = availableResources.Clone();

        // Always allocate Ore first (if possible)
        var oreAlloc = individual.allocations.FirstOrDefault(a => a.objectName == "Ore");
        if (oreAlloc != null)
        {
            int maxOre = CalculateMaxQuantity(oreAlloc.resourceCost, remaining);
            if (maxOre > 0)
            {
                int oreQty = random.Next((int)(maxOre * 0.7f), maxOre + 1); // Favor higher usage
                oreAlloc.quantity = oreQty;
                var used = oreAlloc.GetTotalCost();
                remaining.wood -= used.wood;
                remaining.stone -= used.stone;
                remaining.iron -= used.iron;
                remaining.gold -= used.gold;
                remaining.meat -= used.meat;
            }
        }

        // Then allocate Rock and Pebble
        foreach (var name in new[] { "Rock", "Pebble" })
        {
            var alloc = individual.allocations.FirstOrDefault(a => a.objectName == name);
            if (alloc != null)
            {
                int maxQty = CalculateMaxQuantity(alloc.resourceCost, remaining);
                if (maxQty > 0)
                {
                    int qty = random.Next(0, Mathf.Max(1, (int)(maxQty * 0.9f)));
                    alloc.quantity = qty;
                    var used = alloc.GetTotalCost();
                    remaining.wood -= used.wood;
                    remaining.stone -= used.stone;
                    remaining.iron -= used.iron;
                    remaining.gold -= used.gold;
                    remaining.meat -= used.meat;
                }
            }
        }

        // Continue with the rest of the resource types by priority
        var resourceOrder = GetResourcePriorityOrder().Where(r => r != "iron" && r != "gold" && r != "stone");
        foreach (var resourceType in resourceOrder)
        {
            var objectsForResource = GetObjectsThatUseResource(individual.allocations, resourceType);
            objectsForResource = objectsForResource.OrderBy(x => random.Next()).ToList();

            foreach (var allocation in objectsForResource)
            {
                // Skip Ore, Rock, Pebble (already handled)
                if (allocation.objectName == "Ore" || allocation.objectName == "Rock" || allocation.objectName == "Pebble")
                    continue;

                int maxQuantity = CalculateMaxQuantity(allocation.resourceCost, remaining);
                if (maxQuantity > 0)
                {
                    float priorityMultiplier = GetPriorityMultiplier(resourceType);
                    int quantity = random.Next(0, Mathf.Max(1, (int)(maxQuantity * priorityMultiplier)));
                    allocation.quantity = Mathf.Max(allocation.quantity, quantity);
                    var used = allocation.GetTotalCost();
                    remaining.wood -= used.wood;
                    remaining.stone -= used.stone;
                    remaining.iron -= used.iron;
                    remaining.gold -= used.gold;
                    remaining.meat -= used.meat;
                }
            }
        }

        // In RandomizeWithPriority (after allocating wolves and before allocating deer)
        var wolfAlloc = individual.allocations.FirstOrDefault(a => a.objectName == "Wolf");
        var deerAlloc = individual.allocations.FirstOrDefault(a => a.objectName == "Deer");

        if (wolfAlloc != null && deerAlloc != null)
        {
            int maxDeer = CalculateMaxQuantity(deerAlloc.resourceCost, remaining);
            int minDeer = wolfAlloc.quantity * 3;
            // Ensure at least 3 deer per wolf, but not exceeding available resources
            minDeer = Mathf.Min(minDeer, maxDeer);
            int deerQty = random.Next(minDeer, maxDeer + 1);
            deerAlloc.quantity = deerQty;
            var used = deerAlloc.GetTotalCost();
            remaining.wood -= used.wood;
            remaining.stone -= used.stone;
            remaining.iron -= used.iron;
            remaining.gold -= used.gold;
            remaining.meat -= used.meat;
        }
    }

    List<string> GetResourcePriorityOrder()
    {
        var resources = new List<(string name, int priority)>
        {
            ("meat", meatPriority),
            ("gold", goldPriority),
            ("iron", ironPriority),
            ("stone", stonePriority),
            ("wood", woodPriority)
        };

        return resources.OrderBy(r => r.priority).Select(r => r.name).ToList();
    }

    List<GameObjectAllocation> GetObjectsThatUseResource(List<GameObjectAllocation> allocations, string resourceType)
    {
        return allocations.Where(allocation =>
        {
            switch (resourceType)
            {
                case "meat": return allocation.resourceCost.meat > 0;
                case "gold": return allocation.resourceCost.gold > 0;
                case "iron": return allocation.resourceCost.iron > 0;
                case "stone": return allocation.resourceCost.stone > 0;
                case "wood": return allocation.resourceCost.wood > 0;
                default: return false;
            }
        }).ToList();
    }

    float GetPriorityMultiplier(string resourceType)
    {
        int priority = resourceType switch
        {
            "meat" => meatPriority,
            "gold" => goldPriority,
            "iron" => ironPriority,
            "stone" => stonePriority,
            "wood" => woodPriority,
            _ => 5
        };

        // Higher priority (lower number) = higher multiplier
        return 1.0f - (priority - 1) * 0.15f; // Range from 1.0 to 0.4
    }

    int CalculateMaxQuantity(ResourceDataGA cost, ResourceDataGA available)
    {
        int max = int.MaxValue;

        if (cost.wood > 0) max = Mathf.Min(max, available.wood / cost.wood);
        if (cost.stone > 0) max = Mathf.Min(max, available.stone / cost.stone);
        if (cost.iron > 0) max = Mathf.Min(max, available.iron / cost.iron);
        if (cost.gold > 0) max = Mathf.Min(max, available.gold / cost.gold);
        if (cost.meat > 0) max = Mathf.Min(max, available.meat / cost.meat);

        return max == int.MaxValue ? 0 : Mathf.Max(0, max);
    }

    void EvaluateFitness(Individual individual = null)
    {
        if (individual == null)
        {
            foreach (var ind in population)
            {
                CalculateIndividualFitness(ind);
            }
        }
        else
        {
            CalculateIndividualFitness(individual);
        }
    }

    void CalculateIndividualFitness(Individual individual)
    {
        var used = individual.GetTotalUsedResources();
        var available = availableResources;

        // Check if allocation is valid (not exceeding resources)
        bool isValid = used.wood <= available.wood &&
                      used.stone <= available.stone &&
                      used.iron <= available.iron &&
                      used.gold <= available.gold &&
                      used.meat <= available.meat;

        if (!isValid)
        {
            individual.fitness = 0f;
            return;
        }

        // Calculate resource utilization with priority weights
        float utilization = 0f;

        if (usePriorityAllocation)
        {
            // Weighted utilization based on priority
            if (available.meat > 0)
                utilization += (float)used.meat / available.meat * GetPriorityWeight("meat");
            if (available.gold > 0)
                utilization += (float)used.gold / available.gold * GetPriorityWeight("gold");
            if (available.iron > 0)
                utilization += (float)used.iron / available.iron * GetPriorityWeight("iron");
            if (available.stone > 0)
                utilization += (float)used.stone / available.stone * GetPriorityWeight("stone");
            if (available.wood > 0)
                utilization += (float)used.wood / available.wood * GetPriorityWeight("wood");
        }
        else
        {
            // Equal weight utilization
            if (available.wood > 0) utilization += (float)used.wood / available.wood;
            if (available.stone > 0) utilization += (float)used.stone / available.stone;
            if (available.iron > 0) utilization += (float)used.iron / available.iron;
            if (available.gold > 0) utilization += (float)used.gold / available.gold;
            if (available.meat > 0) utilization += (float)used.meat / available.meat;
        }

        // --- Hybrid GA: reward for low leftover ---
        float totalSupply = available.Total;
        float totalUsed = used.Total;
        float leftoverRatio = totalSupply > 0 ? (totalSupply - totalUsed) / totalSupply : 0f;
        // Reward is higher when leftover is lower (exponential for stronger effect)
        float leftoverReward = 1.0f - leftoverRatio;
        leftoverReward = Mathf.Pow(leftoverReward, 2.0f); // Sharper reward curve

        // Priority bonus - higher bonus for using high-priority resources completely
        float priorityBonus = 0f;
        if (usePriorityAllocation)
        {
            if (used.meat == available.meat) priorityBonus += 2.0f; // Highest priority
            if (used.gold == available.gold) priorityBonus += 1.5f;
            if (used.iron == available.iron) priorityBonus += 1.0f;
            if (used.stone == available.stone) priorityBonus += 0.7f;
            if (used.wood == available.wood) priorityBonus += 0.5f; // Lowest priority
        }
        else
        {
            // Equal completion bonus
            if (used.wood == available.wood) priorityBonus += 0.5f;
            if (used.stone == available.stone) priorityBonus += 0.5f;
            if (used.iron == available.iron) priorityBonus += 0.5f;
            if (used.gold == available.gold) priorityBonus += 0.5f;
            if (used.meat == available.meat) priorityBonus += 0.5f;
        }

        // Diversity bonus (encourage using different object types)
        float diversityBonus = individual.allocations.Count(a => a.quantity > 0) * 0.1f;

        // Add ecosystem balance evaluation
        float ecosystemScore = EvaluateEcosystemBalance(individual);
        
        // Modify final fitness calculation to include ecosystem score
        individual.fitness = (utilization * leftoverReward + priorityBonus + diversityBonus) * (1 + ecosystemScore);

        // --- Ecosystem balance for small vs large objects ---
        int Tree = 3, branch = 0, Rock = 3, pebble = 0, bush = 0;
        foreach (var allocation in individual.allocations)
        {
            switch (allocation.objectName)
            {
                case "Tree": Tree = allocation.quantity; break;
                case "Branch": branch = allocation.quantity; break;
                case "Rock": Rock = allocation.quantity; break;
                case "Pebble": pebble = allocation.quantity; break;
                case "Bush": bush = allocation.quantity; break;
            }
        }

        // Penalty for too many branches vs big trees
        float branchPenalty = 0f;
        if ( branch > Tree * 2)
            branchPenalty = (branch - Tree * 2) * 10f;

        // Penalty for too many pebbles vs Rocks
        float pebblePenalty = 0f;
        if (pebble > Rock * 2)
            pebblePenalty = (pebble - Rock * 2) * 10f;

        // Penalty for too many bushes vs big trees
        float bushPenalty = 0f;
        if (bush > Tree * 3)
            bushPenalty = (bush - Tree * 2) * 1f;

        // Total penalty
        float clutterPenalty = branchPenalty + pebblePenalty + bushPenalty;

        // Subtract penalty from fitness
        individual.fitness -= Math.Abs(clutterPenalty);

        // In CalculateIndividualFitness, after reading wolf and deer quantities:
        int wolves = 0, deer = 0;
        foreach (var allocation in individual.allocations)
        {
            if (allocation.objectName == "Wolf") wolves = allocation.quantity;
            if (allocation.objectName == "Deer") deer = allocation.quantity;
        }
        if (deer < wolves * 2 || deer > wolves *5)
        {
            // Strong penalty if not enough deer for wolves
            individual.fitness -= Math.Abs(wolves * 3 - deer) * 10f;
        }
    }

    float GetPriorityWeight(string resourceType)
    {
        int priority = resourceType switch
        {
            "meat" => meatPriority,
            "gold" => goldPriority,
            "iron" => ironPriority,
            "stone" => stonePriority,
            "wood" => woodPriority,
            _ => 5
        };

        // Higher priority (lower number) = higher weight
        return 6f - priority; // Priority 1 = weight 5, Priority 5 = weight 1
    }

    Individual SelectParent()
    {
        // Tournament selection
        int tournamentSize = 3;
        Individual best = null;

        for (int i = 0; i < tournamentSize; i++)
        {
            var candidate = population[random.Next(population.Count)];
            if (best == null || candidate.fitness > best.fitness)
            {
                best = candidate;
            }
        }

        return best;
    }

    (Individual, Individual) Crossover(Individual parent1, Individual parent2)
    {
        var child1 = parent1.Clone();
        var child2 = parent2.Clone();

        if (random.NextDouble() < crossoverRate)
        {
            // Uniform crossover
            for (int i = 0; i < child1.allocations.Count; i++)
            {
                if (random.NextDouble() < 0.5f)
                {
                    var temp = child1.allocations[i].quantity;
                    child1.allocations[i].quantity = child2.allocations[i].quantity;
                    child2.allocations[i].quantity = temp;
                }
            }
        }

        return (child1, child2);
    }

    void Mutate(Individual individual)
    {
        if (random.NextDouble() < mutationRate)
        {
            // Randomly adjust one allocation
            int index = random.Next(individual.allocations.Count);
            var allocation = individual.allocations[index];

            // Calculate current remaining resources
            var used = individual.GetTotalUsedResources();
            var remaining = new ResourceDataGA(
                availableResources.wood - used.wood + allocation.GetTotalCost().wood,
                availableResources.stone - used.stone + allocation.GetTotalCost().stone,
                availableResources.iron - used.iron + allocation.GetTotalCost().iron,
                availableResources.gold - used.gold + allocation.GetTotalCost().gold,
                availableResources.meat - used.meat + allocation.GetTotalCost().meat
            );

            int maxQuantity = CalculateMaxQuantity(allocation.resourceCost, remaining);
            allocation.quantity = random.Next(0, maxQuantity + 1);
        }
    }

    void DisplayResults()
    {
        Debug.Log("=== RESOURCE ALLOCATION RESULTS ===");

        var used = bestSolution.GetTotalUsedResources();

        Debug.Log($"Fitness Score: {bestSolution.fitness:F2}");
        Debug.Log($"Resource Usage:");
        Debug.Log($"  Wood: {used.wood}/{availableResources.wood} ({(float)used.wood / availableResources.wood * 100:F1}%)");
        Debug.Log($"  Stone: {used.stone}/{availableResources.stone} ({(float)used.stone / availableResources.stone * 100:F1}%)");
        Debug.Log($"  Iron: {used.iron}/{availableResources.iron} ({(float)used.iron / availableResources.iron * 100:F1}%)");
        Debug.Log($"  Gold: {used.gold}/{availableResources.gold} ({(float)used.gold / availableResources.gold * 100:F1}%)");
        Debug.Log($"  Meat: {used.meat}/{availableResources.meat} ({(float)used.meat / availableResources.meat * 100:F1}%)");

        Debug.Log("\nObject Allocations:");
        var resultDict = new Dictionary<string, int>();

        foreach (var allocation in bestSolution.allocations)
        {
            if (allocation.quantity > 0)
            {
                var cost = allocation.GetTotalCost();
                Debug.Log($"  {allocation.objectName}: {allocation.quantity} units (Wood:{cost.wood}, Stone:{cost.stone}, Iron:{cost.iron}, Gold:{cost.gold}, Meat:{cost.meat})");
            }

            // Thêm vào dictionary để lưu (bao gồm cả quantity = 0)
            resultDict[allocation.objectName] = allocation.quantity;
        }

        // Lưu kết quả vào JSON file
        Ingredient.SaveGAResult(resultDict);
    }

    // Public methods for accessing results
    public Dictionary<string, int> GetObjectQuantities()
    {
        var result = new Dictionary<string, int>();

        if (bestSolution != null)
        {
            foreach (var allocation in bestSolution.allocations)
            {
                result[allocation.objectName] = allocation.quantity;
            }
        }

        return result;
    }

    public ResourceDataGA GetUsedResources()
    {
        return bestSolution?.GetTotalUsedResources() ?? new ResourceDataGA();
    }

    public float GetResourceUtilizationPercentage()
    {
        if (bestSolution == null) return 0f;

        var used = bestSolution.GetTotalUsedResources();
        var total = availableResources.Total;
        var usedTotal = used.Total;

        return total > 0 ? (float)usedTotal / total * 100f : 0f;
    }

    // Add this method to calculate actual hunt time based on wolf count
    private float CalculateHuntTime(int wolfCount)
    {
        if (wolfCount <= 0) return 0f;
        float huntTime = baseHuntTime - ((wolfCount - 1) * huntTimeReductionPerWolf);
        return Mathf.Max(huntTime, minHuntTime);
    }

    // Modify EvaluateFitness to include ecosystem balance
    private float EvaluateEcosystemBalance(Individual individual)
    {
        float ecosystemScore = 0f;
        
        // Get wolf and deer counts
        int wolves = 0;
        int deer = 0;
        foreach (var allocation in individual.allocations)
        {
            if (allocation.objectName == "Wolf")
                wolves = allocation.quantity;
            else if (allocation.objectName == "Deer")
                deer = allocation.quantity;
        }

        // Calculate optimal hunt time for current wolf count
        float huntTime = CalculateHuntTime(wolves);
        
        // Calculate theoretical deer consumption rate
        float totalHuntTimePerCycle = huntTime * wolves;
        float theoreticalDeerConsumedPerCycle = totalHuntTimePerCycle > 0 ? 
            wolves * (baseHuntTime / totalHuntTimePerCycle) : 0;

        // Evaluate balance
        if (wolves > 0 && deer > 0)
        {
            // Ideal ratio: wolves should be able to hunt enough deer to sustain themselves
            // but not so many that they quickly eliminate the deer population
            float idealDeerPerWolf = 2.0f; // Each wolf should have access to at least 2 deer
            float actualDeerPerWolf = (float)deer / wolves;
            
            // Calculate balance score
            float ratioScore = 1.0f - Mathf.Abs(actualDeerPerWolf - idealDeerPerWolf) / idealDeerPerWolf;
            ratioScore = Mathf.Max(0, ratioScore);

            // Consider hunt time efficiency
            float huntEfficiencyScore = theoreticalDeerConsumedPerCycle > 0 ? 
                Mathf.Min(1.0f, deer / theoreticalDeerConsumedPerCycle) : 0;

            // Combine scores
            ecosystemScore = (ratioScore + huntEfficiencyScore) / 2.0f;

            // Penalty for extreme cases
            if (wolves > deer * 2) // Too many wolves
                ecosystemScore *= 0.5f;
            else if (wolves * idealDeerPerWolf * 2 < deer) // Too many deer
                ecosystemScore *= 0.7f;
        }
        else if (deer > 0 && wolves == 0)
        {
            // Small penalty for having deer but no wolves
            ecosystemScore = 0.3f;
        }
        else if (wolves > 0 && deer == 0)
        {
            // Major penalty for having wolves but no deer
            ecosystemScore = -0.5f;
        }

        return ecosystemScore;
    }

    // Add this method to update ecosystem state in real-time
    private void UpdateEcosystemState()
    {
        if (!isOptimized || bestSolution == null) return;

        ecosystemState.totalWolves = 0;
        ecosystemState.totalDeer = 0;

        foreach (var allocation in bestSolution.allocations)
        {
            if (allocation.objectName == "Wolf")
                ecosystemState.totalWolves = allocation.quantity;
            else if (allocation.objectName == "Deer")
                ecosystemState.totalDeer = allocation.quantity;
        }

        // Update hunt timer
        if (ecosystemState.isHunting)
        {
            ecosystemState.huntTimer -= Time.deltaTime;
            if (ecosystemState.huntTimer <= 0)
            {
                // Hunt completed - remove one deer if any remain
                if (ecosystemState.totalDeer > 0)
                {
                    ecosystemState.totalDeer--;
                    foreach (var allocation in bestSolution.allocations)
                    {
                        if (allocation.objectName == "Deer")
                        {
                            allocation.quantity = ecosystemState.totalDeer;
                            break;
                        }
                    }
                }
                
                // Reset hunt timer
                ecosystemState.currentHuntTime = CalculateHuntTime(ecosystemState.totalWolves);
                ecosystemState.huntTimer = ecosystemState.currentHuntTime;
            }
        }
        else if (ecosystemState.totalWolves > 0 && ecosystemState.totalDeer > 0)
        {
            // Start new hunt
            ecosystemState.isHunting = true;
            ecosystemState.currentHuntTime = CalculateHuntTime(ecosystemState.totalWolves);
            ecosystemState.huntTimer = ecosystemState.currentHuntTime;
        }
    }
    

    void LoadDefaultTemplate()
    {
        objectTemplate = new List<GameObjectAllocation>
        {
            new GameObjectAllocation("Tree", new ResourceDataGA(20, 0, 0, 0, 0)),
            new GameObjectAllocation("Rock", new ResourceDataGA(0, 15, 2, 0, 0)),
            new GameObjectAllocation("Pebble", new ResourceDataGA(0, 5, 0, 0, 0)),
            new GameObjectAllocation("Branch", new ResourceDataGA(5, 0, 0, 0, 0)),
            new GameObjectAllocation("Bush", new ResourceDataGA(5, 0, 0, 0, 5)),
            new GameObjectAllocation("Ore", new ResourceDataGA(0, 5, 10, 10, 0)),
            new GameObjectAllocation("Wolf", new ResourceDataGA(0, 0, 0, 5, 10)),
            new GameObjectAllocation("Deer", new ResourceDataGA(0, 0, 0, 2, 15))
        };
    }

    // Cập nhật DisplayResults để lưu kết quả vào JSON
    
    void Update()
    {
        UpdateEcosystemState();
    }
}