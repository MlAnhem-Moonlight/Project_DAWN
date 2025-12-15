using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class ResourceDataDE
{
    public int wood = 0;
    public int stone = 0;
    public int iron = 0;
    public int gold = 0;
    public int meat = 0;

    public ResourceDataDE() { }
    public ResourceDataDE(int w, int s, int i, int g, int m)
    {
        wood = w; stone = s; iron = i; gold = g; meat = m;
    }
    public ResourceDataDE Clone() => new ResourceDataDE(wood, stone, iron, gold, meat);
    public int Total => wood + stone + iron + gold + meat;
}

[System.Serializable]
public class GameObjectAllocationDE
{
    public string objectName;
    public ResourceDataDE resourceCost;
    public int quantity;

    public GameObjectAllocationDE(string name, ResourceDataDE cost)
    {
        objectName = name;
        resourceCost = cost;
        quantity = 0;
    }

    public ResourceDataDE GetTotalCost()
    {
        return new ResourceDataDE(
            resourceCost.wood * quantity,
            resourceCost.stone * quantity,
            resourceCost.iron * quantity,
            resourceCost.gold * quantity,
            resourceCost.meat * quantity
        );
    }
}

[System.Serializable]
public class DEIndividual
{
    public float[] genes; // Normalized values [0,1] for each object quantity
    public float fitness;
    public List<GameObjectAllocationDE> allocations;

    public DEIndividual(List<GameObjectAllocationDE> template)
    {
        genes = new float[template.Count];
        allocations = new List<GameObjectAllocationDE>();
        foreach (var allocation in template)
        {
            allocations.Add(new GameObjectAllocationDE(allocation.objectName, allocation.resourceCost));
        }
        fitness = 0f;
    }

    public DEIndividual Clone()
    {
        var clone = new DEIndividual(allocations);
        Array.Copy(genes, clone.genes, genes.Length);
        clone.fitness = fitness;
        for (int i = 0; i < allocations.Count; i++)
        {
            clone.allocations[i].quantity = allocations[i].quantity;
        }
        return clone;
    }
}

public class ResourceAllocationDE : MonoBehaviour
{
    private ResourceSpawnPredictor predictor;
    private IngridientManager ingredientManager;

    [Header("Available Resources")]
    public ResourceDataDE availableResources;

    [Header("DE Parameters")]
    public int populationSize = 50;
    public int generations = 100;
    public float mutationFactor = 0.8f; // F parameter
    public float crossoverRate = 0.9f;  // CR parameter
    public int strategy = 1; // DE strategy variant

    [Header("Resource Priority")]
    public bool usePriorityAllocation = true;
    public int meatPriority = 1;
    public int goldPriority = 2;
    public int ironPriority = 3;
    public int stonePriority = 4;
    public int woodPriority = 5;

    [Header("Results")]
    public DEIndividual bestSolution;
    public bool isOptimized = false;



    private List<GameObjectAllocationDE> objectTemplate;
    private List<DEIndividual> population;
    private int[] maxQuantities; // Cache max quantities for each object
    private System.Random random;

    void Awake()
    {
        predictor = GetComponent<ResourceSpawnPredictor>();
        ingredientManager = GetComponent<IngridientManager>();
    }

    [ContextMenu("Run Differential Evolution")]
    public void RunDE()
    {
        StartCoroutine(WaitForPredictionAndStart());
    }

    IEnumerator WaitForPredictionAndStart()
    {
        yield return null;
        ingredientManager.UpdateResourcePredictor();
        yield return null;

        availableResources = predictor != null ? predictor.PredictionDE() : new ResourceDataDE(300, 250, 100, 180, 350);
        random = new System.Random();
        InitializeObjectTemplate();
        RunDifferentialEvolution();
    }

    // Initialize object templates from JSON data
    void InitializeObjectTemplate()
    {
        objectTemplate = new List<GameObjectAllocationDE>();
        var envData = Ingredient.GetEnvironmentData();

        if (envData?.objects != null)
        {
            foreach (var obj in envData.objects)
            {
                var resourceCost = new ResourceDataDE();
                foreach (var ingredient in obj.ingredients)
                {
                    switch (ingredient.type.ToLower())
                    {
                        case "wood": resourceCost.wood = ingredient.quantity; break;
                        case "stone": resourceCost.stone = ingredient.quantity; break;
                        case "iron": resourceCost.iron = ingredient.quantity; break;
                        case "gold": resourceCost.gold = ingredient.quantity; break;
                        case "meat": resourceCost.meat = ingredient.quantity; break;
                    }
                }
                objectTemplate.Add(new GameObjectAllocationDE(obj.id, resourceCost));
            }
        }
        else
        {
            LoadDefaultTemplate();
        }

        CacheMaxQuantities();
    }

    // Cache maximum possible quantities for each object type
    void CacheMaxQuantities()
    {
        maxQuantities = new int[objectTemplate.Count];
        for (int i = 0; i < objectTemplate.Count; i++)
        {
            maxQuantities[i] = CalculateMaxQuantity(objectTemplate[i].resourceCost, availableResources);
        }
    }

    // Main DE algorithm
    void RunDifferentialEvolution()
    {
        //Debug.Log("Starting Differential Evolution for Resource Allocation...");

        InitializePopulation();

        for (int generation = 0; generation < generations; generation++)
        {
            var newPopulation = new List<DEIndividual>();

            // DE mutation, crossover, and selection for each individual
            for (int i = 0; i < populationSize; i++)
            {
                var target = population[i];
                var mutant = Mutate(i);
                var trial = Crossover(target, mutant);

                DecodeGenes(trial); // Convert genes to actual quantities
                EvaluateFitness(trial);

                // Selection: keep better individual
                newPopulation.Add(trial.fitness > target.fitness ? trial : target);
            }

            population = newPopulation;

            //if (generation % 20 == 0)
            //{
            //    var best = population.OrderByDescending(ind => ind.fitness).First();
            //    //Debug.Log($"Generation {generation}: Best Fitness = {best.fitness:F2}");
            //}
        }

        bestSolution = population.OrderByDescending(ind => ind.fitness).First();
        isOptimized = true;
        DisplayResults();
    }

    // Initialize population with random solutions
    void InitializePopulation()
    {
        population = new List<DEIndividual>();

        for (int i = 0; i < populationSize; i++)
        {
            var individual = new DEIndividual(objectTemplate);
            RandomizeGenes(individual);
            DecodeGenes(individual);
            EvaluateFitness(individual);
            population.Add(individual);
        }
    }

    // Generate random genes with priority-based bias
    void RandomizeGenes(DEIndividual individual)
    {
        if (usePriorityAllocation)
        {
            // Priority-based initialization
            for (int i = 0; i < individual.genes.Length; i++)
            {
                var objName = objectTemplate[i].objectName;
                float priorityBias = GetPriorityBias(objName);

                // Higher priority objects get higher initial values
                individual.genes[i] = Mathf.Clamp01((float)random.NextDouble() * priorityBias);
            }
        }
        else
        {
            for (int i = 0; i < individual.genes.Length; i++)
            {
                individual.genes[i] = (float)random.NextDouble();
            }
        }
    }

    // Get priority bias for object initialization
    float GetPriorityBias(string objectName)
    {
        // Special handling for key objects
        switch (objectName)
        {
            case "Ore": return 1.2f; // Always prioritize
            case "Wolf": return 0.8f; // Moderate priority
            case "Deer": return 0.9f; // High priority for ecosystem
            default:
                // Use resource priority
                var cost = objectTemplate.FirstOrDefault(o => o.objectName == objectName)?.resourceCost;
                if (cost == null) return 0.7f;

                float maxPriority = 0f;
                if (cost.meat > 0) maxPriority = Mathf.Max(maxPriority, GetResourcePriority("meat"));
                if (cost.gold > 0) maxPriority = Mathf.Max(maxPriority, GetResourcePriority("gold"));
                if (cost.iron > 0) maxPriority = Mathf.Max(maxPriority, GetResourcePriority("iron"));
                if (cost.stone > 0) maxPriority = Mathf.Max(maxPriority, GetResourcePriority("stone"));
                if (cost.wood > 0) maxPriority = Mathf.Max(maxPriority, GetResourcePriority("wood"));

                return 0.5f + maxPriority * 0.3f;
        }
    }

    float GetResourcePriority(string resourceType)
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
        return (6f - priority) / 5f; // Normalize to [0.2, 1.0]
    }

    // DE mutation operation
    DEIndividual Mutate(int targetIndex)
    {
        var mutant = new DEIndividual(objectTemplate);

        // Select three random individuals different from target
        int[] indices = new int[3];
        for (int i = 0; i < 3; i++)
        {
            do
            {
                indices[i] = random.Next(populationSize);
            } while (indices[i] == targetIndex || (i > 0 && indices[i] == indices[i - 1]) ||
                     (i > 1 && indices[i] == indices[0]));
        }

        // DE/rand/1 strategy: mutant = x1 + F * (x2 - x3)
        for (int i = 0; i < mutant.genes.Length; i++)
        {
            mutant.genes[i] = population[indices[0]].genes[i] +
                             mutationFactor * (population[indices[1]].genes[i] - population[indices[2]].genes[i]);

            // Boundary constraints
            mutant.genes[i] = Mathf.Clamp01(mutant.genes[i]);
        }

        return mutant;
    }

    // DE crossover operation
    DEIndividual Crossover(DEIndividual target, DEIndividual mutant)
    {
        var trial = target.Clone();
        int jrand = random.Next(target.genes.Length); // Ensure at least one gene from mutant

        for (int i = 0; i < target.genes.Length; i++)
        {
            if (random.NextDouble() < crossoverRate || i == jrand)
            {
                trial.genes[i] = mutant.genes[i];
            }
        }

        return trial;
    }

    // Convert normalized genes to actual object quantities
    void DecodeGenes(DEIndividual individual)
    {
        var remaining = availableResources.Clone();

        // Priority-based decoding with resource constraint handling
        var priorityOrder = GetDecodingOrder();

        foreach (int objIndex in priorityOrder)
        {
            var allocation = individual.allocations[objIndex];

            // Calculate max quantity with current remaining resources
            int maxQty = CalculateMaxQuantity(allocation.resourceCost, remaining);

            // Convert gene to actual quantity
            allocation.quantity = Mathf.RoundToInt(individual.genes[objIndex] * maxQty);

            // Update remaining resources
            var used = allocation.GetTotalCost();
            remaining.wood -= used.wood;
            remaining.stone -= used.stone;
            remaining.iron -= used.iron;
            remaining.gold -= used.gold;
            remaining.meat -= used.meat;
        }

        // Post-process for ecosystem balance
        BalanceEcosystem(individual);
    }

    // Get object decoding order based on priorities
    int[] GetDecodingOrder()
    {
        var order = new List<(int index, float priority)>();

        for (int i = 0; i < objectTemplate.Count; i++)
        {
            var objName = objectTemplate[i].objectName;
            float priority = GetPriorityBias(objName);
            order.Add((i, priority));
        }

        return order.OrderByDescending(x => x.priority).Select(x => x.index).ToArray();
    }

    // Ensure ecosystem balance between wolves and deer
    void BalanceEcosystem(DEIndividual individual)
    {
        int wolfIndex = -1, deerIndex = -1;

        for (int i = 0; i < individual.allocations.Count; i++)
        {
            if (individual.allocations[i].objectName == "Wolf") wolfIndex = i;
            if (individual.allocations[i].objectName == "Deer") deerIndex = i;
        }

        if (wolfIndex >= 0 && deerIndex >= 0)
        {
            int wolves = individual.allocations[wolfIndex].quantity;
            int deer = individual.allocations[deerIndex].quantity;

            // Ensure minimum deer count for wolves
            if (deer < wolves * 2)
            {
                var remaining = GetRemainingAfterAllocation(individual, deerIndex);
                int maxDeer = CalculateMaxQuantity(individual.allocations[deerIndex].resourceCost, remaining);
                individual.allocations[deerIndex].quantity = Mathf.Min(wolves * 3, maxDeer);
            }
        }
    }

    // Get remaining resources after current allocation (excluding specified object)
    ResourceDataDE GetRemainingAfterAllocation(DEIndividual individual, int excludeIndex)
    {
        var used = new ResourceDataDE();
        for (int i = 0; i < individual.allocations.Count; i++)
        {
            if (i == excludeIndex) continue;
            var cost = individual.allocations[i].GetTotalCost();
            used.wood += cost.wood;
            used.stone += cost.stone;
            used.iron += cost.iron;
            used.gold += cost.gold;
            used.meat += cost.meat;
        }

        return new ResourceDataDE(
            availableResources.wood - used.wood,
            availableResources.stone - used.stone,
            availableResources.iron - used.iron,
            availableResources.gold - used.gold,
            availableResources.meat - used.meat
        );
    }

    // Calculate fitness using same evaluation as GA
    void EvaluateFitness(DEIndividual individual)
    {
        var used = GetTotalUsedResources(individual);
        var available = availableResources;

        // Validity check
        bool isValid = used.wood <= available.wood && used.stone <= available.stone &&
                      used.iron <= available.iron && used.gold <= available.gold && used.meat <= available.meat;

        if (!isValid)
        {
            individual.fitness = 0f;
            return;
        }

        // Resource utilization with priority weights
        float utilization = 0f;
        if (usePriorityAllocation)
        {
            if (available.meat > 0) utilization += (float)used.meat / available.meat * GetPriorityWeight("meat");
            if (available.gold > 0) utilization += (float)used.gold / available.gold * GetPriorityWeight("gold");
            if (available.iron > 0) utilization += (float)used.iron / available.iron * GetPriorityWeight("iron");
            if (available.stone > 0) utilization += (float)used.stone / available.stone * GetPriorityWeight("stone");
            if (available.wood > 0) utilization += (float)used.wood / available.wood * GetPriorityWeight("wood");
        }
        else
        {
            if (available.wood > 0) utilization += (float)used.wood / available.wood;
            if (available.stone > 0) utilization += (float)used.stone / available.stone;
            if (available.iron > 0) utilization += (float)used.iron / available.iron;
            if (available.gold > 0) utilization += (float)used.gold / available.gold;
            if (available.meat > 0) utilization += (float)used.meat / available.meat;
        }

        // Leftover penalty
        float totalSupply = available.Total;
        float totalUsed = used.Total;
        float leftoverRatio = totalSupply > 0 ? (totalSupply - totalUsed) / totalSupply : 0f;
        float leftoverReward = Mathf.Pow(1.0f - leftoverRatio, 2.0f);

        // Priority completion bonus
        float priorityBonus = CalculatePriorityBonus(used, available);

        // Diversity and ecosystem bonuses
        float diversityBonus = individual.allocations.Count(a => a.quantity > 0) * 0.1f;
        float ecosystemScore = EvaluateEcosystemBalance(individual);

        // Clutter penalties
        float clutterPenalty = CalculateClutterPenalty(individual);

        individual.fitness = (utilization * leftoverReward + priorityBonus + diversityBonus) * (1 + ecosystemScore) - clutterPenalty;
    }

    ResourceDataDE GetTotalUsedResources(DEIndividual individual)
    {
        var total = new ResourceDataDE();
        foreach (var allocation in individual.allocations)
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

    float CalculatePriorityBonus(ResourceDataDE used, ResourceDataDE available)
    {
        float priorityBonus = 0f;
        if (usePriorityAllocation)
        {
            if (used.meat == available.meat) priorityBonus += 2.0f;
            if (used.gold == available.gold) priorityBonus += 1.5f;
            if (used.iron == available.iron) priorityBonus += 1.0f;
            if (used.stone == available.stone) priorityBonus += 0.7f;
            if (used.wood == available.wood) priorityBonus += 0.5f;
        }
        else
        {
            if (used.wood == available.wood) priorityBonus += 0.5f;
            if (used.stone == available.stone) priorityBonus += 0.5f;
            if (used.iron == available.iron) priorityBonus += 0.5f;
            if (used.gold == available.gold) priorityBonus += 0.5f;
            if (used.meat == available.meat) priorityBonus += 0.5f;
        }
        return priorityBonus;
    }

    float CalculateClutterPenalty(DEIndividual individual)
    {
        int tree = 3, branch = 0, rock = 3, pebble = 0, bush = 0;

        foreach (var allocation in individual.allocations)
        {
            switch (allocation.objectName)
            {
                case "Tree": tree = allocation.quantity; break;
                case "Branch": branch = allocation.quantity; break;
                case "Rock": rock = allocation.quantity; break;
                case "Pebble": pebble = allocation.quantity; break;
                case "Bush": bush = allocation.quantity; break;
            }
        }

        float penalty = 0f;
        if (branch > tree * 2) penalty += (branch - tree * 2) * 10f;
        if (pebble > rock * 2) penalty += (pebble - rock * 2) * 10f;
        if (bush > tree * 3) penalty += (bush - tree * 2) * 1f;

        return penalty;
    }

    float EvaluateEcosystemBalance(DEIndividual individual)
    {
        int wolves = 0, deer = 0;
        foreach (var allocation in individual.allocations)
        {
            if (allocation.objectName == "Wolf") wolves = allocation.quantity;
            if (allocation.objectName == "Deer") deer = allocation.quantity;
        }

        if (deer < wolves * 2 || deer > wolves * 5)
        {
            return -Math.Abs(wolves * 3 - deer) * 0.01f;
        }

        // Ecosystem balance scoring (same as GA)
        if (wolves > 0 && deer > 0)
        {
            float idealDeerPerWolf = 2.0f;
            float actualDeerPerWolf = (float)deer / wolves;
            float ratioScore = 1.0f - Mathf.Abs(actualDeerPerWolf - idealDeerPerWolf) / idealDeerPerWolf;
            return Mathf.Max(0, ratioScore) * 0.3f;
        }

        return wolves == 0 && deer > 0 ? 0.1f : -0.2f;
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
        return 6f - priority;
    }

    int CalculateMaxQuantity(ResourceDataDE cost, ResourceDataDE available)
    {
        int max = int.MaxValue;
        if (cost.wood > 0) max = Mathf.Min(max, available.wood / cost.wood);
        if (cost.stone > 0) max = Mathf.Min(max, available.stone / cost.stone);
        if (cost.iron > 0) max = Mathf.Min(max, available.iron / cost.iron);
        if (cost.gold > 0) max = Mathf.Min(max, available.gold / cost.gold);
        if (cost.meat > 0) max = Mathf.Min(max, available.meat / cost.meat);
        return max == int.MaxValue ? 0 : Mathf.Max(0, max);
    }

    void DisplayResults()
    {
        Debug.Log("=== DIFFERENTIAL EVOLUTION RESULTS ===");
        var used = GetTotalUsedResources(bestSolution);

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
                ////Debug.Log($"  {allocation.objectName}: {allocation.quantity} units (Wood:{cost.wood}, Stone:{cost.stone}, Iron:{cost.iron}, Gold:{cost.gold}, Meat:{cost.meat})");
            }
            resultDict[allocation.objectName] = allocation.quantity;
        }

        Ingredient.SaveGAResult(resultDict);
    }

    void LoadDefaultTemplate()
    {
        objectTemplate = new List<GameObjectAllocationDE>
        {
            new GameObjectAllocationDE("Tree", new ResourceDataDE(20, 0, 0, 0, 0)),
            new GameObjectAllocationDE("Rock", new ResourceDataDE(0, 15, 2, 0, 0)),
            new GameObjectAllocationDE("Pebble", new ResourceDataDE(0, 5, 0, 0, 0)),
            new GameObjectAllocationDE("Branch", new ResourceDataDE(5, 0, 0, 0, 0)),
            new GameObjectAllocationDE("Bush", new ResourceDataDE(5, 0, 0, 0, 5)),
            new GameObjectAllocationDE("Ore", new ResourceDataDE(0, 5, 10, 10, 0)),
            new GameObjectAllocationDE("Wolf", new ResourceDataDE(0, 0, 0, 5, 10)),
            new GameObjectAllocationDE("Deer", new ResourceDataDE(0, 0, 0, 2, 15))
        };
    }

    // Public interface methods (same as GA)
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

    public ResourceDataDE GetUsedResources() => bestSolution != null ? GetTotalUsedResources(bestSolution) : new ResourceDataDE();

    public float GetResourceUtilizationPercentage()
    {
        if (bestSolution == null) return 0f;
        var used = GetTotalUsedResources(bestSolution);
        var total = availableResources.Total;
        var usedTotal = used.Total;
        return total > 0 ? (float)usedTotal / total * 100f : 0f;
    }
}