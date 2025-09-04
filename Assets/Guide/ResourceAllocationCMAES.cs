using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ResourceAllocationCMAES
/// - Drop-in thay thế cho GA, dùng (separable) CMA-ES để tối ưu phân bổ số lượng object.
/// - Giữ nguyên mô hình dữ liệu ResourceDataGA, GameObjectAllocation, Individual.
/// - Sinh nghiệm từ phân phối Gaussian, làm tròn về số nguyên, rồi sửa-feasible theo ràng buộc tài nguyên.
/// - Fitness: tái sử dụng cùng logic như GA (utilization + priority + ecosystem balance + penalty).
/// </summary>
public class ResourceAllocationCMAES : MonoBehaviour
{
    // ====== Dependencies & Data ======
    private ResourceSpawnPredictor predictor;
    private IngridientManager ingredientManager;

    [Header("Available Resources")]
    public ResourceDataGA availableResources;

    [Header("Priority (1=highest, 5=lowest)")]
    public bool usePriorityAllocation = true;
    public int meatPriority = 1;
    public int goldPriority = 2;
    public int ironPriority = 3;
    public int stonePriority = 4;
    public int woodPriority = 5;

    [Header("CMA-ES Parameters")]
    [Tooltip("Số cá thể mỗi vòng lặp (lambda).")]
    public int populationSize = 48; // lambda
    [Tooltip("Số cá thể tốt nhất để tạo mean mới (mu). Mặc định = lambda/2.")]
    public int parentCount = 0;     // mu (0 => auto lambda/2)
    [Tooltip("Số vòng lặp (thế hệ).")]
    public int generations = 100;
    [Tooltip("Bước khởi tạo (step-size).")]
    public float initSigma = 1.5f;
    [Tooltip("Hệ số cập nhật phương sai (separable CMA-ES). 0.05~0.3.")]
    public float cCov = 0.2f;
    [Tooltip("Điều chỉnh sigma theo quy tắc 1/5 thành công.")]
    public float sigmaIncrease = 1.2f;
    public float sigmaDecrease = 0.82f;

    [Header("Ecosystem Balance Parameters")]
    [SerializeField] private float baseHuntTime = 80f;
    [SerializeField] private float huntTimeReductionPerWolf = 10f;
    [SerializeField] private float minHuntTime = 20f;

    [Header("Results")]
    public Individual bestSolution;
    public bool isOptimized = false;

    // ====== Internal ======
    private System.Random rng;
    private List<GameObjectAllocation> objectTemplate;
    private int n;                       // dimension = số loại object
    private double[] mean;               // vector mean (liên tục)
    private double[] varDiag;            // phương sai từng chiều (separable CMA-ES)
    private double sigma;                // step-size
    private double bestFitness = double.NegativeInfinity;
    private List<(Individual ind, double fit)> lastPopulation = new List<(Individual, double)>();

    // Ecosystem runtime state (giống GA)
    private struct EcosystemState
    {
        public int totalWolves;
        public int totalDeer;
        public float currentHuntTime;
        public float huntTimer;
        public bool isHunting;
    }
    private EcosystemState ecosystemState;

    // ====== Unity Hooks ======
    void Awake()
    {
        predictor = GetComponent<ResourceSpawnPredictor>();
        ingredientManager = GetComponent<IngridientManager>();
    }

    [ContextMenu("Run CMA-ES")]
    public void RunCMAES()
    {
        StartCoroutine(WaitPredictionAndRun());
    }

    /// <summary>
    /// Đợi predictor sẵn sàng, load template, rồi chạy CMA-ES.
    /// </summary>
    IEnumerator WaitPredictionAndRun()
    {
        yield return null;
        if (ingredientManager != null) ingredientManager.UpdateResourcePredictor();
        yield return null;

        availableResources = predictor != null ? predictor.Prediction() : new ResourceDataGA(300, 250, 100, 180, 350);
        rng = new System.Random();
        InitializeObjectTemplate();
        InitializeCMAES();
        RunOptimization();
        DisplayResults();
    }

    void Update()
    {
        UpdateEcosystemState();
    }

    // ====== Setup ======

    /// <summary>
    /// Load object template từ JSON (giống GA). Fallback sang default nếu lỗi.
    /// </summary>
    void InitializeObjectTemplate()
    {
        objectTemplate = new List<GameObjectAllocation>();
        var envData = Ingredient.GetEnvironmentData();

        if (envData != null && envData.objects != null)
        {
            foreach (var obj in envData.objects)
            {
                var resourceCost = new ResourceDataGA();
                foreach (var ing in obj.ingredients)
                {
                    switch (ing.type.ToLower())
                    {
                        case "wood":  resourceCost.wood  = ing.quantity; break;
                        case "stone": resourceCost.stone = ing.quantity; break;
                        case "iron":  resourceCost.iron  = ing.quantity; break;
                        case "gold":  resourceCost.gold  = ing.quantity; break;
                        case "meat":  resourceCost.meat  = ing.quantity; break;
                    }
                }
                objectTemplate.Add(new GameObjectAllocation(obj.id, resourceCost));
            }
            Debug.Log($"[CMA-ES] Loaded {objectTemplate.Count} templates from JSON");
        }
        else
        {
            Debug.LogWarning("[CMA-ES] Failed to load JSON. Using default template.");
            LoadDefaultTemplate();
        }
    }

    /// <summary>
    /// Khởi tạo tham số CMA-ES: mean, varDiag, sigma, parentCount, weights.
    /// Mean khởi tạo dựa trên heuristic tính max quantity khả dụng cho từng object.
    /// </summary>
    void InitializeCMAES()
    {
        n = objectTemplate.Count;
        mean = new double[n];
        varDiag = new double[n];
        sigma = initSigma;
        if (parentCount <= 0) parentCount = Mathf.Max(1, populationSize / 2);

        // Heuristic: set mean gần ~ 30-50% max feasible của từng object để hội tụ nhanh.
        for (int i = 0; i < n; i++)
        {
            var alloc = objectTemplate[i];
            int maxQ = CalculateMaxQuantity(alloc.resourceCost, availableResources);
            mean[i] = Math.Max(0, maxQ * 0.4);         // continuous mean
            varDiag[i] = Math.Max(1e-6, Math.Pow(Math.Max(1.0, maxQ * 0.2), 2)); // phương sai khởi tạo
        }
    }

    // ====== Optimization Loop ======

    /// <summary>
    /// Vòng tối ưu CMA-ES (separable): sinh quần thể, evaluate, chọn top-mu, cập nhật mean/var, điều chỉnh sigma.
    /// </summary>
    void RunOptimization()
    {
        // Log-weights cho tái tổ hợp (chuẩn CMA-ES)
        int mu = parentCount;
        double[] weights = Enumerable.Range(0, mu)
            .Select(i => Math.Log(mu + 0.5) - Math.Log(i + 1))
            .ToArray();
        double weightSum = weights.Sum();
        for (int i = 0; i < mu; i++) weights[i] /= weightSum;

        double prevBest = double.NegativeInfinity;
        int successCount = 0;

        for (int gen = 0; gen < generations; gen++)
        {
            lastPopulation.Clear();

            // 1) Sample lambda cá thể từ N(mean, diag(varDiag)*sigma^2)
            for (int k = 0; k < populationSize; k++)
            {
                double[] y = new double[n];
                for (int d = 0; d < n; d++)
                {
                    // Box-constrained sampling: đảm bảo không âm bằng cách dịch biên hợp lý
                    double std = Math.Sqrt(varDiag[d]) * sigma;
                    double sample = mean[d] + std * NextGaussian(rng);
                    y[d] = Math.Max(0.0, sample); // không âm
                }

                // 2) Map -> Individual nguyên + sửa-feasible
                Individual ind = VectorToFeasibleIndividual(y);

                // 3) Evaluate fitness (dùng cùng logic GA)
                CalculateIndividualFitness(ind);
                lastPopulation.Add((ind, ind.fitness));
            }

            // 4) Chọn top-mu, cập nhật mean/var (separable)
            var sorted = lastPopulation.OrderByDescending(t => t.fit).ToList();
            var elites = sorted.Take(mu).ToList();
            double[] oldMean = (double[])mean.Clone();

            for (int d = 0; d < n; d++)
            {
                // mean mới = tổng w_i * x_i(d)
                mean[d] = elites.Select((t, i) => weights[i] * (double)t.ind.allocations[d].quantity).Sum();

                // cập nhật phương sai theo khoảng cách với oldMean (rank-μ update, separable)
                double v = 0.0;
                for (int i = 0; i < mu; i++)
                {
                    double diff = elites[i].ind.allocations[d].quantity - oldMean[d];
                    v += weights[i] * diff * diff;
                }
                varDiag[d] = (1.0 - cCov) * varDiag[d] + cCov * Math.Max(1e-9, v);
            }

            // 5) Điều chỉnh sigma (1/5 success rule)
            double bestThisGen = sorted[0].fit;
            if (bestThisGen > prevBest) successCount++;
            if ((gen + 1) % 5 == 0)
            {
                double ratio = successCount / 5.0;
                sigma = (ratio > 0.2) ? sigma * sigmaIncrease : sigma * sigmaDecrease;
                sigma = Math.Max(0.1, Math.Min(10.0, sigma));
                successCount = 0;
            }
            prevBest = Math.Max(prevBest, bestThisGen);

            // 6) Lưu best global
            if (bestThisGen > bestFitness)
            {
                bestFitness = bestThisGen;
                bestSolution = sorted[0].ind.Clone();
            }

            if (gen % 20 == 0)
                Debug.Log($"[CMA-ES] Gen {gen} | BestFit {bestThisGen:F3} | Sigma {sigma:F2}");
        }

        isOptimized = true;
    }

    // ====== Mapping & Feasibility ======

    /// <summary>
    /// Chuyển vector thực -> Individual nguyên, rồi sửa để không vượt tài nguyên.
    /// </summary>
    Individual VectorToFeasibleIndividual(double[] y)
    {
        var ind = new Individual(objectTemplate);
        // 1) Làm tròn, clamp 0..max theo tài nguyên hiện có (ước lượng từng object)
        for (int i = 0; i < n; i++)
        {
            int q = Mathf.Max(0, Mathf.RoundToInt((float)y[i]));
            ind.allocations[i].quantity = q;
        }

        // 2) Nếu vượt tài nguyên, giảm bớt theo heuristic (giảm thứ dùng nhiều tài nguyên nhất trước)
        MakeFeasible(ind);
        return ind;
    }

    /// <summary>
    /// Heuristic sửa-feasible: trong khi vượt tài nguyên, giảm 1 đơn vị ở object có tổng cost lớn nhất còn dương.
    /// </summary>
    void MakeFeasible(Individual ind)
    {
        for (int guard = 0; guard < 100000; guard++)
        {
            var used = ind.GetTotalUsedResources();
            if (used.wood <= availableResources.wood &&
                used.stone <= availableResources.stone &&
                used.iron <= availableResources.iron &&
                used.gold <= availableResources.gold &&
                used.meat <= availableResources.meat)
            {
                break;
            }

            // Tìm object đang còn quantity > 0, có "tổng chi phí" lớn nhất để giảm trước
            int worstIdx = -1;
            int worstCost = -1;
            for (int i = 0; i < n; i++)
            {
                var a = ind.allocations[i];
                if (a.quantity <= 0) continue;
                int costSum = a.resourceCost.wood + a.resourceCost.stone + a.resourceCost.iron + a.resourceCost.gold + a.resourceCost.meat;
                if (costSum > worstCost) { worstCost = costSum; worstIdx = i; }
            }
            if (worstIdx < 0) break; // không thể giảm thêm

            ind.allocations[worstIdx].quantity -= 1;
        }
    }

    // ====== Fitness (same logic as GA) ======

    void CalculateIndividualFitness(Individual individual)
    {
        var used = individual.GetTotalUsedResources();
        var available = availableResources;

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

        float utilization = 0f;
        if (usePriorityAllocation)
        {
            if (available.meat  > 0) utilization += (float)used.meat  / available.meat  * GetPriorityWeight("meat");
            if (available.gold  > 0) utilization += (float)used.gold  / available.gold  * GetPriorityWeight("gold");
            if (available.iron  > 0) utilization += (float)used.iron  / available.iron  * GetPriorityWeight("iron");
            if (available.stone > 0) utilization += (float)used.stone / available.stone * GetPriorityWeight("stone");
            if (available.wood  > 0) utilization += (float)used.wood  / available.wood  * GetPriorityWeight("wood");
        }
        else
        {
            if (available.wood  > 0) utilization += (float)used.wood  / available.wood;
            if (available.stone > 0) utilization += (float)used.stone / available.stone;
            if (available.iron  > 0) utilization += (float)used.iron  / available.iron;
            if (available.gold  > 0) utilization += (float)used.gold  / available.gold;
            if (available.meat  > 0) utilization += (float)used.meat  / available.meat;
        }

        float totalSupply = available.Total;
        float totalUsed = used.Total;
        float leftoverRatio = totalSupply > 0 ? (totalSupply - totalUsed) / totalSupply : 0f;
        float leftoverReward = Mathf.Pow(1.0f - leftoverRatio, 2.0f);

        float priorityBonus = 0f;
        if (usePriorityAllocation)
        {
            if (used.meat == available.meat)   priorityBonus += 2.0f;
            if (used.gold == available.gold)   priorityBonus += 1.5f;
            if (used.iron == available.iron)   priorityBonus += 1.0f;
            if (used.stone == available.stone) priorityBonus += 0.7f;
            if (used.wood == available.wood)   priorityBonus += 0.5f;
        }
        else
        {
            if (used.wood  == available.wood)  priorityBonus += 0.5f;
            if (used.stone == available.stone) priorityBonus += 0.5f;
            if (used.iron  == available.iron)  priorityBonus += 0.5f;
            if (used.gold  == available.gold)  priorityBonus += 0.5f;
            if (used.meat  == available.meat)  priorityBonus += 0.5f;
        }

        float diversityBonus = individual.allocations.Count(a => a.quantity > 0) * 0.1f;

        float ecosystemScore = EvaluateEcosystemBalance(individual);
        individual.fitness = (utilization * leftoverReward + priorityBonus + diversityBonus) * (1 + ecosystemScore);

        // Penalties (Tree/Branch/Rock/Pebble/Bush) + Wolf-Deer ratio (giữ nguyên logic)
        int Tree = 3, branch = 0, Rock = 3, pebble = 0, bush = 0;
        foreach (var a in individual.allocations)
        {
            switch (a.objectName)
            {
                case "Tree":   Tree = a.quantity; break;
                case "Branch": branch = a.quantity; break;
                case "Rock":   Rock = a.quantity; break;
                case "Pebble": pebble = a.quantity; break;
                case "Bush":   bush = a.quantity; break;
            }
        }
        float branchPenalty = (branch > Tree * 2)  ? (branch - Tree * 2) * 10f : 0f;
        float pebblePenalty = (pebble > Rock * 2)  ? (pebble - Rock * 2) * 10f : 0f;
        float bushPenalty   = (bush > Tree * 3)    ? (bush - Tree * 2) * 1f   : 0f;
        float clutterPenalty = branchPenalty + pebblePenalty + bushPenalty;
        individual.fitness -= Math.Abs(clutterPenalty);

        int wolves = 0, deer = 0;
        foreach (var a in individual.allocations)
        {
            if (a.objectName == "Wolf") wolves = a.quantity;
            if (a.objectName == "Deer") deer = a.quantity;
        }
        if (deer < wolves * 2 || deer > wolves * 5)
        {
            individual.fitness -= Math.Abs(wolves * 3 - deer) * 10f;
        }
    }

    float GetPriorityWeight(string resourceType)
    {
        int p = resourceType switch
        {
            "meat" => meatPriority,
            "gold" => goldPriority,
            "iron" => ironPriority,
            "stone" => stonePriority,
            "wood" => woodPriority,
            _ => 5
        };
        return 6f - p; // priority 1 => weight 5, priority 5 => weight 1
    }

    // ====== Ecosystem helpers (same as GA) ======

    private float CalculateHuntTime(int wolfCount)
    {
        if (wolfCount <= 0) return 0f;
        float huntTime = baseHuntTime - ((wolfCount - 1) * huntTimeReductionPerWolf);
        return Mathf.Max(huntTime, minHuntTime);
    }

    private float EvaluateEcosystemBalance(Individual individual)
    {
        float ecosystemScore = 0f;
        int wolves = 0, deer = 0;
        foreach (var a in individual.allocations)
        {
            if (a.objectName == "Wolf") wolves = a.quantity;
            else if (a.objectName == "Deer") deer = a.quantity;
        }

        float huntTime = CalculateHuntTime(wolves);
        float totalHuntTimePerCycle = huntTime * wolves;
        float theoreticalDeerConsumedPerCycle = totalHuntTimePerCycle > 0 ?
            wolves * (baseHuntTime / totalHuntTimePerCycle) : 0;

        if (wolves > 0 && deer > 0)
        {
            float idealDeerPerWolf = 2.0f;
            float actualDeerPerWolf = (float)deer / wolves;
            float ratioScore = 1.0f - Mathf.Abs(actualDeerPerWolf - idealDeerPerWolf) / idealDeerPerWolf;
            ratioScore = Mathf.Max(0, ratioScore);

            float huntEfficiencyScore = theoreticalDeerConsumedPerCycle > 0 ?
                Mathf.Min(1.0f, deer / theoreticalDeerConsumedPerCycle) : 0;

            ecosystemScore = (ratioScore + huntEfficiencyScore) / 2.0f;

            if (wolves > deer * 2) ecosystemScore *= 0.5f;
            else if (wolves * idealDeerPerWolf * 2 < deer) ecosystemScore *= 0.7f;
        }
        else if (deer > 0 && wolves == 0)
        {
            ecosystemScore = 0.3f;
        }
        else if (wolves > 0 && deer == 0)
        {
            ecosystemScore = -0.5f;
        }
        return ecosystemScore;
    }

    private void UpdateEcosystemState()
    {
        if (!isOptimized || bestSolution == null) return;
        ecosystemState.totalWolves = 0;
        ecosystemState.totalDeer = 0;

        foreach (var a in bestSolution.allocations)
        {
            if (a.objectName == "Wolf") ecosystemState.totalWolves = a.quantity;
            else if (a.objectName == "Deer") ecosystemState.totalDeer = a.quantity;
        }

        if (ecosystemState.isHunting)
        {
            ecosystemState.huntTimer -= Time.deltaTime;
            if (ecosystemState.huntTimer <= 0)
            {
                if (ecosystemState.totalDeer > 0)
                {
                    ecosystemState.totalDeer--;
                    foreach (var a in bestSolution.allocations)
                    {
                        if (a.objectName == "Deer")
                        {
                            a.quantity = ecosystemState.totalDeer;
                            break;
                        }
                    }
                }
                ecosystemState.currentHuntTime = CalculateHuntTime(ecosystemState.totalWolves);
                ecosystemState.huntTimer = ecosystemState.currentHuntTime;
            }
        }
        else if (ecosystemState.totalWolves > 0 && ecosystemState.totalDeer > 0)
        {
            ecosystemState.isHunting = true;
            ecosystemState.currentHuntTime = CalculateHuntTime(ecosystemState.totalWolves);
            ecosystemState.huntTimer = ecosystemState.currentHuntTime;
        }
    }

    // ====== Utils & IO ======

    /// <summary>
    /// Max quantity theo tài nguyên còn (giống GA).
    /// </summary>
    int CalculateMaxQuantity(ResourceDataGA cost, ResourceDataGA available)
    {
        int max = int.MaxValue;
        if (cost.wood  > 0) max = Mathf.Min(max, available.wood  / cost.wood);
        if (cost.stone > 0) max = Mathf.Min(max, available.stone / cost.stone);
        if (cost.iron  > 0) max = Mathf.Min(max, available.iron  / cost.iron);
        if (cost.gold  > 0) max = Mathf.Min(max, available.gold  / cost.gold);
        if (cost.meat  > 0) max = Mathf.Min(max, available.meat  / cost.meat);
        return max == int.MaxValue ? 0 : Mathf.Max(0, max);
    }

    void DisplayResults()
    {
        if (bestSolution == null)
        {
            Debug.LogWarning("[CMA-ES] No solution found.");
            return;
        }

        var used = bestSolution.GetTotalUsedResources();
        Debug.Log("=== CMA-ES RESOURCE ALLOCATION RESULTS ===");
        Debug.Log($"Fitness Score: {bestSolution.fitness:F2}");
        Debug.Log($"  Wood: {used.wood}/{availableResources.wood} ({(float)used.wood / availableResources.wood * 100f:F1}%)");
        Debug.Log($"  Stone: {used.stone}/{availableResources.stone} ({(float)used.stone / availableResources.stone * 100f:F1}%)");
        Debug.Log($"  Iron: {used.iron}/{availableResources.iron} ({(float)used.iron / availableResources.iron * 100f:F1}%)");
        Debug.Log($"  Gold: {used.gold}/{availableResources.gold} ({(float)used.gold / availableResources.gold * 100f:F1}%)");
        Debug.Log($"  Meat: {used.meat}/{availableResources.meat} ({(float)used.meat / availableResources.meat * 100f:F1}%)");

        var resultDict = new Dictionary<string, int>();
        foreach (var a in bestSolution.allocations)
        {
            if (a.quantity > 0)
            {
                var cost = a.GetTotalCost();
                Debug.Log($"  {a.objectName}: {a.quantity} units (Wood:{cost.wood}, Stone:{cost.stone}, Iron:{cost.iron}, Gold:{cost.gold}, Meat:{cost.meat})");
            }
            resultDict[a.objectName] = a.quantity;
        }
        Ingredient.SaveGAResult(resultDict);
    }

    void LoadDefaultTemplate()
    {
        objectTemplate = new List<GameObjectAllocation>
        {
            new GameObjectAllocation("Tree",   new ResourceDataGA(20, 0, 0, 0, 0)),
            new GameObjectAllocation("Rock",   new ResourceDataGA(0, 15, 2, 0, 0)),
            new GameObjectAllocation("Pebble", new ResourceDataGA(0, 5, 0, 0, 0)),
            new GameObjectAllocation("Branch", new ResourceDataGA(5, 0, 0, 0, 0)),
            new GameObjectAllocation("Bush",   new ResourceDataGA(5, 0, 0, 0, 5)),
            new GameObjectAllocation("Ore",    new ResourceDataGA(0, 5, 10, 10, 0)),
            new GameObjectAllocation("Wolf",   new ResourceDataGA(0, 0, 0, 5, 10)),
            new GameObjectAllocation("Deer",   new ResourceDataGA(0, 0, 0, 2, 15))
        };
    }

    // Gaussian helper
    static double NextGaussian(System.Random rng)
    {
        // Box-Muller transform
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        double r = Math.Sqrt(-2.0 * Math.Log(u1));
        double theta = 2.0 * Math.PI * u2;
        return r * Math.Cos(theta);
    }
}
