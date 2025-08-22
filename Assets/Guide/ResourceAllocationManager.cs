using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ResourceAllocationGA
{
    // Định nghĩa các loại đối tượng có thể sinh ra
    public enum GameObjectType
    {
        DaTang,     // Đá tảng
        Soi,        // Sỏi
        CanhCay,    // Cành cây
        CayTo,      // Cây to
        Quang,      // Quặng
        BuiCay,     // Bụi cây
        Huou,       // Hươu
        Soi_Animal  // Sói (động vật)
    }

    // Cấu trúc tài nguyên đầu vào
    [System.Serializable]
    public class Resources
    {
        public int Wood;    // Gỗ
        public int Stone;   // Đá
        public int Iron;    // Sắt
        public int Gold;    // Vàng
        public int Meat;    // Thịt

        public Resources(int wood, int stone, int iron, int gold, int meat)
        {
            Wood = wood;
            Stone = stone;
            Iron = iron;
            Gold = gold;
            Meat = meat;
        }

        public Resources Clone()
        {
            return new Resources(Wood, Stone, Iron, Gold, Meat);
        }
    }

    // Cấu trúc chi phí tài nguyên cho mỗi đối tượng
    [System.Serializable]
    public class ObjectCost
    {
        public GameObjectType ObjectType;
        public Resources Cost;
        public int Priority; // Độ ưu tiên (1-10)

        public ObjectCost(GameObjectType type, Resources cost, int priority = 5)
        {
            ObjectType = type;
            Cost = cost;
            Priority = priority;
        }
    }

    // Cấu trúc kết quả phân bổ
    public class AllocationResult
    {
        public Dictionary<GameObjectType, int> ObjectCounts;
        public Resources RemainingResources;
        public double Fitness;

        public AllocationResult()
        {
            ObjectCounts = new Dictionary<GameObjectType, int>();
            RemainingResources = new Resources(0, 0, 0, 0, 0);
        }
    }

    // Nhiễm sắc thể (chromosome) đại diện cho một giải pháp phân bổ
    public class Chromosome
    {
        public Dictionary<GameObjectType, int> Genes;
        public double Fitness;

        public Chromosome()
        {
            Genes = new Dictionary<GameObjectType, int>();
            foreach (GameObjectType type in System.Enum.GetValues(typeof(GameObjectType)))
            {
                Genes[type] = 0;
            }
        }

        public Chromosome Clone()
        {
            var clone = new Chromosome();
            foreach (var kvp in Genes)
            {
                clone.Genes[kvp.Key] = kvp.Value;
            }
            clone.Fitness = Fitness;
            return clone;
        }
    }

    public class ResourceAllocationGAEngine
    {
        private readonly System.Random _random;
        private readonly Dictionary<GameObjectType, ObjectCost> _objectCosts;
        private Resources _availableResources;

        // Tham số GA
        private int _populationSize = 100;
        private int _generations = 500;
        private double _mutationRate = 0.1;
        private double _crossoverRate = 0.8;

        public ResourceAllocationGAEngine()
        {
            _random = new System.Random();
            _objectCosts = new Dictionary<GameObjectType, ObjectCost>();
            InitializeObjectCosts();
        }

        // Khởi tạo chi phí tài nguyên cho mỗi đối tượng (có thể điều chỉnh)
        private void InitializeObjectCosts()
        {
            _objectCosts[GameObjectType.DaTang] = new ObjectCost(
                GameObjectType.DaTang,
                new Resources(0, 15, 2, 0, 0), 6);

            _objectCosts[GameObjectType.Soi] = new ObjectCost(
                GameObjectType.Soi,
                new Resources(0, 5, 0, 0, 0), 4);

            _objectCosts[GameObjectType.CanhCay] = new ObjectCost(
                GameObjectType.CanhCay,
                new Resources(5, 0, 0, 0, 0), 5);

            _objectCosts[GameObjectType.CayTo] = new ObjectCost(
                GameObjectType.CayTo,
                new Resources(20, 0, 0, 0, 0), 8);

            _objectCosts[GameObjectType.Quang] = new ObjectCost(
                GameObjectType.Quang,
                new Resources(0, 10, 10, 5, 0), 7);

            _objectCosts[GameObjectType.BuiCay] = new ObjectCost(
                GameObjectType.BuiCay,
                new Resources(5, 0, 0, 0, 5), 3);

            _objectCosts[GameObjectType.Huou] = new ObjectCost(
                GameObjectType.Huou,
                new Resources(0, 0, 0, 2, 20), 7);

            _objectCosts[GameObjectType.Soi_Animal] = new ObjectCost(
                GameObjectType.Soi_Animal,
                new Resources(0, 0, 0, 5, 10), 5);
        }

        // Cập nhật chi phí tài nguyên cho đối tượng
        public void UpdateObjectCost(GameObjectType objectType, Resources cost, int priority = 5)
        {
            _objectCosts[objectType] = new ObjectCost(objectType, cost, priority);
        }

        // Hàm chính để tối ưu phân bổ tài nguyên
        public AllocationResult OptimizeAllocation(Resources availableResources)
        {
            _availableResources = availableResources.Clone();

            var population = InitializePopulation();

            for (int generation = 0; generation < _generations; generation++)
            {
                // Đánh giá fitness cho tất cả cá thể
                foreach (var chromosome in population)
                {
                    chromosome.Fitness = CalculateFitness(chromosome);
                }

                // Sắp xếp theo fitness giảm dần
                population = population.OrderByDescending(c => c.Fitness).ToList();

                // Tạo thế hệ mới
                var newPopulation = new List<Chromosome>();

                // Giữ lại 10% cá thể tốt nhất (elitism)
                int eliteCount = _populationSize / 10;
                for (int i = 0; i < eliteCount; i++)
                {
                    newPopulation.Add(population[i].Clone());
                }

                // Tạo phần còn lại thông qua crossover và mutation
                while (newPopulation.Count < _populationSize)
                {
                    var parent1 = SelectParent(population);
                    var parent2 = SelectParent(population);

                    var children = Crossover(parent1, parent2);

                    foreach (var child in children)
                    {
                        if (newPopulation.Count >= _populationSize) break;

                        Mutate(child);
                        newPopulation.Add(child);
                    }
                }

                population = newPopulation;

                // In tiến trình mỗi 50 thế hệ
                if (generation % 50 == 0)
                {
                    Debug.Log($"Generation {generation}: Best Fitness = {population.Max(c => c.Fitness):F2}");
                }
            }

            // Trả về kết quả tốt nhất
            foreach (var chromosome in population)
            {
                chromosome.Fitness = CalculateFitness(chromosome);
            }

            var best = population.OrderByDescending(c => c.Fitness).First();
            return CreateAllocationResult(best);
        }

        // Khởi tạo quần thể ban đầu
        private List<Chromosome> InitializePopulation()
        {
            var population = new List<Chromosome>();

            for (int i = 0; i < _populationSize; i++)
            {
                var chromosome = new Chromosome();
                var remainingResources = _availableResources.Clone();

                foreach (GameObjectType objectType in System.Enum.GetValues(typeof(GameObjectType)))
                {
                    int maxPossible = CalculateMaxPossible(objectType, remainingResources);
                    chromosome.Genes[objectType] = _random.Next(0, Math.Max(1, maxPossible + 1));

                    var cost = _objectCosts[objectType].Cost;
                    remainingResources.Wood -= chromosome.Genes[objectType] * cost.Wood;
                    remainingResources.Stone -= chromosome.Genes[objectType] * cost.Stone;
                    remainingResources.Iron -= chromosome.Genes[objectType] * cost.Iron;
                    remainingResources.Gold -= chromosome.Genes[objectType] * cost.Gold;
                    remainingResources.Meat -= chromosome.Genes[objectType] * cost.Meat;
                }

                population.Add(chromosome);
            }

            return population;
        }

        // Tính số lượng tối đa có thể tạo ra cho một đối tượng
        private int CalculateMaxPossible(GameObjectType objectType, Resources resources)
        {
            var cost = _objectCosts[objectType].Cost;
            int max = int.MaxValue;

            if (cost.Wood > 0) max = Math.Min(max, resources.Wood / cost.Wood);
            if (cost.Stone > 0) max = Math.Min(max, resources.Stone / cost.Stone);
            if (cost.Iron > 0) max = Math.Min(max, resources.Iron / cost.Iron);
            if (cost.Gold > 0) max = Math.Min(max, resources.Gold / cost.Gold);
            if (cost.Meat > 0) max = Math.Min(max, resources.Meat / cost.Meat);

            return Math.Max(0, max == int.MaxValue ? 10 : max);
        }

        // Tính fitness của một nhiễm sắc thể
        private double CalculateFitness(Chromosome chromosome)
        {
            var usedResources = new Resources(0, 0, 0, 0, 0);
            double totalValue = 0;

            // Tính tổng tài nguyên sử dụng và giá trị
            foreach (var kvp in chromosome.Genes)
            {
                var cost = _objectCosts[kvp.Key].Cost;
                var count = kvp.Value;

                usedResources.Wood += count * cost.Wood;
                usedResources.Stone += count * cost.Stone;
                usedResources.Iron += count * cost.Iron;
                usedResources.Gold += count * cost.Gold;
                usedResources.Meat += count * cost.Meat;

                totalValue += count * _objectCosts[kvp.Key].Priority;
            }

            // Kiểm tra vi phạm ràng buộc tài nguyên
            if (usedResources.Wood > _availableResources.Wood ||
                usedResources.Stone > _availableResources.Stone ||
                usedResources.Iron > _availableResources.Iron ||
                usedResources.Gold > _availableResources.Gold ||
                usedResources.Meat > _availableResources.Meat)
            {
                return 0; // Fitness = 0 nếu vi phạm ràng buộc
            }

            // Áp dụng ràng buộc sinh thái: sói ăn hươu
            int deer = chromosome.Genes[GameObjectType.Huou];
            int wolves = chromosome.Genes[GameObjectType.Soi_Animal];
            if (deer > 0)
            {
                totalValue += 20; // thưởng có Hươu
            }
            // Nếu có nhiều sói hơn hươu thì hươu bị ăn hết
            if (wolves > deer)
            {
                totalValue -= (wolves - deer) * 2; // Penalty cho mất cân bằng sinh thái
            }

            // Thêm bonus cho việc sử dụng hiệu quả tài nguyên
            double resourceUtilization = (double)(usedResources.Wood + usedResources.Stone +
                                                 usedResources.Iron + usedResources.Gold + usedResources.Meat) /
                                        (_availableResources.Wood + _availableResources.Stone +
                                         _availableResources.Iron + _availableResources.Gold + _availableResources.Meat);

            return totalValue + resourceUtilization * 10;
        }

        // Chọn cha mẹ cho crossover (tournament selection)
        private Chromosome SelectParent(List<Chromosome> population)
        {
            int tournamentSize = 5;
            var tournament = new List<Chromosome>();

            for (int i = 0; i < tournamentSize; i++)
            {
                tournament.Add(population[_random.Next(population.Count)]);
            }

            return tournament.OrderByDescending(c => c.Fitness).First();
        }

        // Lai ghép (crossover)
        private List<Chromosome> Crossover(Chromosome parent1, Chromosome parent2)
        {
            var child1 = parent1.Clone();
            var child2 = parent2.Clone();

            if (_random.NextDouble() < _crossoverRate)
            {
                var objectTypes = new List<GameObjectType>((GameObjectType[])System.Enum.GetValues(typeof(GameObjectType)));
                int crossoverPoint = _random.Next(1, objectTypes.Count);

                for (int i = crossoverPoint; i < objectTypes.Count; i++)
                {
                    var temp = child1.Genes[objectTypes[i]];
                    child1.Genes[objectTypes[i]] = child2.Genes[objectTypes[i]];
                    child2.Genes[objectTypes[i]] = temp;
                }
            }

            return new List<Chromosome> { child1, child2 };
        }

        // Đột biến (mutation)
        private void Mutate(Chromosome chromosome)
        {
            if (_random.NextDouble() < _mutationRate)
            {
                var objectTypes = new List<GameObjectType>((GameObjectType[])System.Enum.GetValues(typeof(GameObjectType)));
                var randomType = objectTypes[_random.Next(objectTypes.Count)];

                // Tăng hoặc giảm số lượng ngẫu nhiên (trong giới hạn hợp lý)
                int change = _random.Next(-2, 3); // -2, -1, 0, 1, 2
                chromosome.Genes[randomType] = Math.Max(0, chromosome.Genes[randomType] + change);
            }
        }

        // Tạo kết quả phân bổ từ nhiễm sắc thể tốt nhất
        private AllocationResult CreateAllocationResult(Chromosome bestChromosome)
        {
            var result = new AllocationResult();
            result.Fitness = bestChromosome.Fitness;
            result.ObjectCounts = new Dictionary<GameObjectType, int>(bestChromosome.Genes);

            // Tính tài nguyên còn lại
            result.RemainingResources = _availableResources.Clone();
            foreach (var kvp in bestChromosome.Genes)
            {
                var cost = _objectCosts[kvp.Key].Cost;
                var count = kvp.Value;

                result.RemainingResources.Wood -= count * cost.Wood;
                result.RemainingResources.Stone -= count * cost.Stone;
                result.RemainingResources.Iron -= count * cost.Iron;
                result.RemainingResources.Gold -= count * cost.Gold;
                result.RemainingResources.Meat -= count * cost.Meat;
            }

            return result;
        }

        // Cấu hình tham số GA
        public void ConfigureGA(int populationSize, int generations, double mutationRate, double crossoverRate)
        {
            _populationSize = populationSize;
            _generations = generations;
            _mutationRate = mutationRate;
            _crossoverRate = crossoverRate;
        }
    }
}

// MonoBehaviour component để sử dụng trong Unity
public class ResourceAllocationManager : MonoBehaviour
{
    [Header("Tài Nguyên Đầu Vào")]
    [SerializeField] private int wood = 50;
    [SerializeField] private int stone = 30;
    [SerializeField] private int iron = 20;
    [SerializeField] private int gold = 10;
    [SerializeField] private int meat = 25;

    [Header("Cấu Hình Genetic Algorithm")]
    [SerializeField] private int populationSize = 150;
    [SerializeField] private int generations = 300;
    [SerializeField] private float mutationRate = 0.15f;
    [SerializeField] private float crossoverRate = 0.85f;

    [Header("Tùy Chọn")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool showDetailedResults = true;

    private ResourceAllocationGA.ResourceAllocationGAEngine gaEngine;

    void Start()
    {
        if (runOnStart)
        {
            RunOptimization();
        }
    }

    [ContextMenu("Chạy Tối Ưu Hóa")]
    public void RunOptimization()
    {
        Debug.Log("=== GENETIC ALGORITHM - PHÂN BỔ TÀI NGUYÊN GAME ===");

        // Khởi tạo GA engine
        gaEngine = new ResourceAllocationGA.ResourceAllocationGAEngine();

        // Cấu hình tham số GA
        gaEngine.ConfigureGA(populationSize, generations, mutationRate, crossoverRate);

        // Tùy chỉnh chi phí tài nguyên (ví dụ)
        gaEngine.UpdateObjectCost(ResourceAllocationGA.GameObjectType.CayTo,
            new ResourceAllocationGA.Resources(4, 1, 0, 0, 0), 9);
        gaEngine.UpdateObjectCost(ResourceAllocationGA.GameObjectType.Soi_Animal,
            new ResourceAllocationGA.Resources(0, 0, 1, 1, 4), 7);

        // Tạo tài nguyên đầu vào
        var availableResources = new ResourceAllocationGA.Resources(wood, stone, iron, gold, meat);

        Debug.Log("Tài nguyên đầu vào:");
        Debug.Log($"Gỗ: {availableResources.Wood}, Đá: {availableResources.Stone}, " +
                  $"Sắt: {availableResources.Iron}, Vàng: {availableResources.Gold}, Thịt: {availableResources.Meat}");

        Debug.Log("Bắt đầu tối ưu hóa...");

        // Chạy thuật toán tối ưu hóa
        var result = gaEngine.OptimizeAllocation(availableResources);

        // Hiển thị kết quả
        ShowResults(result);
    }

    private void ShowResults(ResourceAllocationGA.AllocationResult result)
    {
        Debug.Log("=== KẾT QUẢ PHÂN BỔ TỐI ƯU ===");
        Debug.Log($"Fitness Score: {result.Fitness:F2}");
        Debug.Log("Số lượng đối tượng được sinh ra:");

        foreach (var kvp in result.ObjectCounts)
        {
            if (kvp.Value > 0)
            {
                string name = kvp.Key switch
                {
                    ResourceAllocationGA.GameObjectType.DaTang => "Đá Tảng",
                    ResourceAllocationGA.GameObjectType.Soi => "Sỏi",
                    ResourceAllocationGA.GameObjectType.CanhCay => "Cành Cây",
                    ResourceAllocationGA.GameObjectType.CayTo => "Cây To",
                    ResourceAllocationGA.GameObjectType.Quang => "Quặng",
                    ResourceAllocationGA.GameObjectType.BuiCay => "Bụi Cây",
                    ResourceAllocationGA.GameObjectType.Huou => "Hươu",
                    ResourceAllocationGA.GameObjectType.Soi_Animal => "Sói",
                    _ => kvp.Key.ToString()
                };
                Debug.Log($"  {name}: {kvp.Value}");
            }
        }

        Debug.Log("Tài nguyên còn lại:");
        Debug.Log($"Gỗ: {result.RemainingResources.Wood}, Đá: {result.RemainingResources.Stone}, " +
                  $"Sắt: {result.RemainingResources.Iron}, Vàng: {result.RemainingResources.Gold}, " +
                  $"Thịt: {result.RemainingResources.Meat}");

        // Kiểm tra cân bằng sinh thái
        int deer = result.ObjectCounts[ResourceAllocationGA.GameObjectType.Huou];
        int wolves = result.ObjectCounts[ResourceAllocationGA.GameObjectType.Soi_Animal];
        if (deer > 0 && wolves > 0)
        {
            Debug.Log($"Cân bằng sinh thái: {deer} hươu vs {wolves} sói");
            if (wolves <= deer)
            {
                Debug.Log("✓ Hệ sinh thái cân bằng");
            }
            else
            {
                Debug.Log("⚠ Cảnh báo: Quá nhiều sói so với hươu!");
            }
        }

        if (showDetailedResults)
        {
            ShowDetailedAnalysis(result);
        }
    }

    private void ShowDetailedAnalysis(ResourceAllocationGA.AllocationResult result)
    {
        Debug.Log("=== PHÂN TÍCH CHI TIẾT ===");

        // Tính tổng tài nguyên sử dụng
        int totalUsedWood = wood - result.RemainingResources.Wood;
        int totalUsedStone = stone - result.RemainingResources.Stone;
        int totalUsedIron = iron - result.RemainingResources.Iron;
        int totalUsedGold = gold - result.RemainingResources.Gold;
        int totalUsedMeat = meat - result.RemainingResources.Meat;

        Debug.Log("Tỷ lệ sử dụng tài nguyên:");
        if (wood > 0) Debug.Log($"Gỗ: {(float)totalUsedWood / wood * 100:F1}% ({totalUsedWood}/{wood})");
        if (stone > 0) Debug.Log($"Đá: {(float)totalUsedStone / stone * 100:F1}% ({totalUsedStone}/{stone})");
        if (iron > 0) Debug.Log($"Sắt: {(float)totalUsedIron / iron * 100:F1}% ({totalUsedIron}/{iron})");
        if (gold > 0) Debug.Log($"Vàng: {(float)totalUsedGold / gold * 100:F1}% ({totalUsedGold}/{gold})");
        if (meat > 0) Debug.Log($"Thịt: {(float)totalUsedMeat / meat * 100:F1}% ({totalUsedMeat}/{meat})");

        // Tính tổng số đối tượng
        int totalObjects = result.ObjectCounts.Values.Sum();
        Debug.Log($"Tổng số đối tượng được tạo: {totalObjects}");
    }

    // Hàm để gọi từ UI hoặc các script khác
    public void UpdateResources(int newWood, int newStone, int newIron, int newGold, int newMeat)
    {
        wood = newWood;
        stone = newStone;
        iron = newIron;
        gold = newGold;
        meat = newMeat;

        Debug.Log($"Tài nguyên đã được cập nhật: Gỗ={wood}, Đá={stone}, Sắt={iron}, Vàng={gold}, Thịt={meat}");
    }

    public void UpdateGAParameters(int newPopulationSize, int newGenerations, float newMutationRate, float newCrossoverRate)
    {
        populationSize = newPopulationSize;
        generations = newGenerations;
        mutationRate = newMutationRate;
        crossoverRate = newCrossoverRate;

        Debug.Log($"Tham số GA đã được cập nhật: Population={populationSize}, Generations={generations}, MutationRate={mutationRate}, CrossoverRate={crossoverRate}");
    }
}