using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum UnitFaction { Enemy, Ally }

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyPool
    {
        public string name;
        public GameObject prefab;
        public int poolSize = 10;
        public UnitFaction faction = UnitFaction.Enemy; // Enemy or Ally

        [HideInInspector]
        public List<GameObject> pool = new List<GameObject>();
    }

    [Header("Danh sách các loại Enemy")]
    public List<EnemyPool> enemyPools = new List<EnemyPool>();

    [Header("Điểm spawn (nếu null sẽ random)")]
    public Transform spawnPoint;

    [Header("Level Constraints")]
    [Tooltip("Level tối thiểu của enemy")]
    public int minEnemyLevel = 1;

    [Tooltip("Level tối đa của enemy")]
    public int maxEnemyLevel = 8;

    [Header("Spawn Limits")]
    public int maxUnitCanSpawn = 0;

    private void Awake()
    {
        // Tạo object pool cho từng loại enemy
        foreach (var pool in enemyPools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning($"⚠️ EnemyPool '{pool.name}' chưa có prefab!");
                continue;
            }

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                pool.pool.Add(obj);
            }
        }
    }

    //private void Update()
    //{

    //}

    public bool CheckSpawnLimit()
    {
        // Lấy toàn bộ BuildConstruction trong scene
        BuildConstruction[] buildings =
            FindObjectsByType<BuildConstruction>(FindObjectsSortMode.None);

        // Lọc theo điều kiện:
        // 1. isBuilt == true
        // 2. buildingType == VillagerHouse hoặc Fortress
        List<BuildConstruction> validBuildings = buildings
            .Where(b => b.isBuilt &&
                       (b.buildingType == BuildConstruction.BuildingType.VillagerHouse ||
                        b.buildingType == BuildConstruction.BuildingType.Fortress))
            .ToList();

        maxUnitCanSpawn += validBuildings.Count * 10;

        Debug.Log("Số lượng quân lính có thể sử dụng: " + maxUnitCanSpawn);
        if (enemyPools.Count < maxUnitCanSpawn) return true;
        else return false;
    }


    /// <summary>
    /// Spawn enemy với level cụ thể
    /// type: loại spawn (1=single, 2=group, 3=mixed)
    /// difficultyLevel: số lượng/composition
    /// enemyLevel: level của enemy (1-20)
    /// </summary>
    public void SpawnEnemy(int type, int difficultyLevel, int enemyLevel)
    {
        Debug.Log($"{this.name}");
        switch (type)
        {
            case 1:
                SpawnType1(difficultyLevel, enemyLevel);
                break;
            case 2:
                SpawnType2(difficultyLevel, enemyLevel);
                break;
            case 3:
                SpawnType3(difficultyLevel, enemyLevel);
                break;
            default:
                Debug.LogWarning($"⚠️ Unknown spawn type: {type}");
                break;
        }
    }


    //ally
    //Gọi hàm này để spawn ally từ pool
    public void SpawnAlly(string allyName, int allyLevel)
    {
        if (!EnsureInstance()) return;

        var pool = enemyPools.Find(p => p.name == allyName && p.faction == UnitFaction.Ally);
        if (pool == null)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy Ally '{allyName}' trong pool!");
            return;
        }

        SpawnUnitFromPool(pool, spawnPoint != null ? spawnPoint.position : GetRandomSpawn(), allyLevel);
    }

    private void SpawnUnitFromPool(EnemyPool pool, Vector3 spawnPos, int level)
    {
        foreach (var unit in pool.pool)
        {
            if (!unit.activeInHierarchy)
            {
                unit.transform.position = spawnPos;
                unit.SetActive(true);

                var stats = unit.GetComponent<Stats>();
                if (stats != null)
                {
                    stats.level = level;
                    stats.ApplyGrowth();
                }

                // Nếu là Ally, bật AI thân thiện hoặc đánh theo lệnh
                // Nếu là Enemy, AI vẫn như cũ
                return;
            }
        }

        Debug.LogWarning($"⚠️ Hết unit trong pool '{pool.name}'");
    }


    // ===============================
    // 🔥 3 kiểu spawn với level system
    // ===============================

    /// <summary>
    /// 🧩 Kiểu 1: Spawn 1 enemy duy nhất
    /// difficultyLevel ảnh hưởng đến variance của level
    /// LƯU Ý: chỉ chọn pool có faction == Enemy
    /// </summary>
    [ContextMenu("Spawn Type 1 (Single Enemy)")]
    public void SpawnType1(int difficultyLevel = 1, int enemyLevel = 1)
    {
        Debug.Log("Spawn Type 1 called: ");
        if (!EnsureInstance()) return;

        var enemyOnlyPools = enemyPools.Where(p => p.faction == UnitFaction.Enemy).ToList();
        if (enemyOnlyPools.Count == 0)
        {
            Debug.LogWarning("⚠️ Không có Enemy pool để spawn (tất cả là Ally?)");
            return;
        }

        var pool = enemyOnlyPools[Random.Range(0, enemyOnlyPools.Count)];

        // Thêm variance dựa trên difficulty
        int variance = Mathf.Clamp(difficultyLevel - 1, 0, 3);
        int finalLevel = Mathf.Clamp(enemyLevel + Random.Range(-variance, variance + 1),
                                     minEnemyLevel, maxEnemyLevel);

        SpawnEnemyFromPool(pool, finalLevel);
        Debug.Log($"⚔️ SpawnType1: 1 enemy ({pool.name}) level {finalLevel} (base: {enemyLevel}, difficulty: {difficultyLevel})");
    }

    /// <summary>
    /// 🧟 Kiểu 2: Spawn nhiều enemy cùng loại
    /// difficultyLevel quyết định số lượng
    /// enemyLevel quyết định sức mạnh
    /// </summary>
    [ContextMenu("Spawn Type 2 (Scaled Group)")]
    public void SpawnType2(int difficultyLevel = 1, int enemyLevel = 1)
    {
        if (!EnsureInstance()) return;

        var enemyOnlyPools = enemyPools.Where(p => p.faction == UnitFaction.Enemy).ToList();
        if (enemyOnlyPools.Count == 0)
        {
            Debug.LogWarning("⚠️ Không có Enemy pool để spawn (tất cả là Ally?)");
            return;
        }

        var pool = enemyOnlyPools[Random.Range(0, enemyOnlyPools.Count)];

        // Số lượng tỉ lệ với difficulty
        int count = Mathf.Clamp(difficultyLevel * 2, 2, pool.poolSize);

        int spawned = 0;
        foreach (var enemy in pool.pool)
        {
            if (!enemy.activeInHierarchy)
            {
                Vector3 pos = spawnPoint != null ? spawnPoint.position : GetRandomSpawn();
                enemy.transform.position = pos + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                enemy.SetActive(true);

                var stats = enemy.GetComponent<Stats>();
                if (stats != null)
                {
                    // Variance nhỏ cho level của từng con
                    int finalLevel = Mathf.Clamp(enemyLevel + Random.Range(-1, 2),
                                                 minEnemyLevel, maxEnemyLevel);
                    stats.level = finalLevel;
                    stats.ApplyGrowth();
                }

                spawned++;
                if (spawned >= count) break;
            }
        }

        Debug.Log($"⚔️ SpawnType2: {spawned} enemies ({pool.name}) level ~{enemyLevel} (difficulty: {difficultyLevel})");
    }

    /// <summary>
    /// 🧨 Kiểu 3: Spawn đội hình hỗn hợp
    /// difficultyLevel quyết định số lượng và đa dạng
    /// enemyLevel quyết định sức mạnh trung bình
    /// </summary>
    [ContextMenu("Spawn Type 3 (Mixed Group)")]
    public void SpawnType3(int difficultyLevel = 1, int enemyLevel = 1)
    {
        if (!EnsureInstance()) return;

        // Số lượng tăng theo difficulty
        int count = Mathf.Clamp(2 + difficultyLevel, 2, 8);
        int spawned = 0;

        var enemyOnlyPools = enemyPools.Where(p => p.faction == UnitFaction.Enemy).ToList();
        if (enemyOnlyPools.Count == 0)
        {
            Debug.LogWarning("⚠️ Không có Enemy pool để spawn (tất cả là Ally?)");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var pool = enemyOnlyPools[Random.Range(0, enemyOnlyPools.Count)];

            foreach (var enemy in pool.pool)
            {
                if (!enemy.activeInHierarchy)
                {
                    Vector3 pos = spawnPoint != null ? spawnPoint.position : GetRandomSpawn();
                    enemy.transform.position = pos + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                    enemy.SetActive(true);

                    var stats = enemy.GetComponent<Stats>();
                    if (stats != null)
                    {
                        // Tạo variance để có enemy mạnh/yếu khác nhau trong đội
                        int levelVariance = Random.Range(-2, 3);
                        int finalLevel = Mathf.Clamp(enemyLevel + levelVariance,
                                                     minEnemyLevel, maxEnemyLevel);
                        stats.level = finalLevel;
                        stats.ApplyGrowth();
                    }

                    spawned++;
                    break;
                }
            }
        }

        Debug.Log($"⚔️ SpawnType3: {spawned} mixed enemies level ~{enemyLevel} (difficulty: {difficultyLevel})");
    }

    // ===============================
    // ⚙️ Tiện ích chung
    // ===============================

    private bool EnsureInstance()
    {
        if (enemyPools == null || enemyPools.Count == 0)
        {
            Debug.LogWarning("⚠️ Không có EnemyPool nào trong EnemySpawner!");
            return false;
        }

        return true;
    }

    private void SpawnEnemyFromPool(EnemyPool pool, int level)
    {
        foreach (var enemy in pool.pool)
        {
            if (!enemy.activeInHierarchy)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : GetRandomSpawn();
                enemy.transform.position = spawnPos;
                enemy.SetActive(true);

                var stats = enemy.GetComponent<Stats>();
                if (stats != null)
                {
                    stats.level = Mathf.Clamp(level, minEnemyLevel, maxEnemyLevel);
                    stats.ApplyGrowth();
                }
                return;
            }
        }
    }

    private Vector3 GetRandomSpawn()
    {
        return new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
    }

    // ===============================
    // 🧪 Debug & Testing
    // ===============================

    [ContextMenu("Test Spawn - Early Game (Lv3)")]
    public void TestSpawnEarlyGame()
    {
        SpawnEnemy(3, 1, 3); // Mixed, difficulty 1, level 3
    }

    [ContextMenu("Test Spawn - Mid Game (Lv10)")]
    public void TestSpawnMidGame()
    {
        SpawnEnemy(2, 2, 10); // Group, difficulty 2, level 10
    }

    [ContextMenu("Test Spawn - Late Game (Lv17)")]
    public void TestSpawnLateGame()
    {
        SpawnEnemy(3, 3, 17); // Mixed, difficulty 3, level 17
    }

    [ContextMenu("Test Spawn - End Game (Lv20)")]
    public void TestSpawnEndGame()
    {
        SpawnEnemy(2, 4, 20); // Group, difficulty 4, level 20
    }
}