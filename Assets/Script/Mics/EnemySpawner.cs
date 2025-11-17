using System.Collections.Generic;
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
    public int maxEnemyLevel = 20;

    private static EnemySpawner instance;

    private void Awake()
    {
        instance = this;

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

    /// <summary>
    /// Spawn enemy với level cụ thể
    /// type: loại spawn (1=single, 2=group, 3=mixed)
    /// difficultyLevel: số lượng/composition
    /// enemyLevel: level của enemy (1-20)
    /// </summary>
    public static void SpawnEnemy(int type, int difficultyLevel, int enemyLevel)
    {
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
    public static void SpawnAlly(string allyName, Vector3 spawnPosition, int allyLevel)
    {
        if (!EnsureInstance()) return;

        var pool = instance.enemyPools.Find(p => p.name == allyName && p.faction == UnitFaction.Ally);
        if (pool == null)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy Ally '{allyName}' trong pool!");
            return;
        }

        SpawnUnitFromPool(pool, spawnPosition, allyLevel);
    }

    private static void SpawnUnitFromPool(EnemyPool pool, Vector3 spawnPos, int level)
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
    /// </summary>
    [ContextMenu("Spawn Type 1 (Single Enemy)")]
    public static void SpawnType1(int difficultyLevel = 1, int enemyLevel = 1)
    {
        if (!EnsureInstance()) return;

        var pool = instance.enemyPools[Random.Range(0, instance.enemyPools.Count)];

        // Thêm variance dựa trên difficulty
        int variance = Mathf.Clamp(difficultyLevel - 1, 0, 3);
        int finalLevel = Mathf.Clamp(enemyLevel + Random.Range(-variance, variance + 1),
                                     instance.minEnemyLevel, instance.maxEnemyLevel);

        SpawnEnemyFromPool(pool, finalLevel);
        Debug.Log($"⚔️ SpawnType1: 1 enemy ({pool.name}) level {finalLevel} (base: {enemyLevel}, difficulty: {difficultyLevel})");
    }

    /// <summary>
    /// 🧟 Kiểu 2: Spawn nhiều enemy cùng loại
    /// difficultyLevel quyết định số lượng
    /// enemyLevel quyết định sức mạnh
    /// </summary>
    [ContextMenu("Spawn Type 2 (Scaled Group)")]
    public static void SpawnType2(int difficultyLevel = 1, int enemyLevel = 1)
    {
        if (!EnsureInstance()) return;

        var pool = instance.enemyPools[Random.Range(0, instance.enemyPools.Count)];

        // Số lượng tỉ lệ với difficulty
        int count = Mathf.Clamp(difficultyLevel * 2, 2, pool.poolSize);

        int spawned = 0;
        foreach (var enemy in pool.pool)
        {
            if (!enemy.activeInHierarchy)
            {
                Vector3 pos = instance.spawnPoint != null ? instance.spawnPoint.position : GetRandomSpawn();
                enemy.transform.position = pos + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                enemy.SetActive(true);

                var stats = enemy.GetComponent<Stats>();
                if (stats != null)
                {
                    // Variance nhỏ cho level của từng con
                    int finalLevel = Mathf.Clamp(enemyLevel + Random.Range(-1, 2),
                                                 instance.minEnemyLevel, instance.maxEnemyLevel);
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
    public static void SpawnType3(int difficultyLevel = 1, int enemyLevel = 1)
    {
        if (!EnsureInstance()) return;

        // Số lượng tăng theo difficulty
        int count = Mathf.Clamp(2 + difficultyLevel, 2, 8);
        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            var pool = instance.enemyPools[Random.Range(0, instance.enemyPools.Count)];

            foreach (var enemy in pool.pool)
            {
                if (!enemy.activeInHierarchy)
                {
                    Vector3 pos = instance.spawnPoint != null ? instance.spawnPoint.position : GetRandomSpawn();
                    enemy.transform.position = pos + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                    enemy.SetActive(true);

                    var stats = enemy.GetComponent<Stats>();
                    if (stats != null)
                    {
                        // Tạo variance để có enemy mạnh/yếu khác nhau trong đội
                        int levelVariance = Random.Range(-2, 3);
                        int finalLevel = Mathf.Clamp(enemyLevel + levelVariance,
                                                     instance.minEnemyLevel, instance.maxEnemyLevel);
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

    private static bool EnsureInstance()
    {
        if (instance == null)
        {
            Debug.LogError("❌ EnemySpawner not found in scene!");
            return false;
        }

        if (instance.enemyPools.Count == 0)
        {
            Debug.LogWarning("⚠️ Không có EnemyPool nào trong EnemySpawner!");
            return false;
        }

        return true;
    }

    private static void SpawnEnemyFromPool(EnemyPool pool, int level)
    {
        foreach (var enemy in pool.pool)
        {
            if (!enemy.activeInHierarchy)
            {
                Vector3 spawnPos = instance.spawnPoint != null ? instance.spawnPoint.position : GetRandomSpawn();
                enemy.transform.position = spawnPos;
                enemy.SetActive(true);

                var stats = enemy.GetComponent<Stats>();
                if (stats != null)
                {
                    stats.level = Mathf.Clamp(level, instance.minEnemyLevel, instance.maxEnemyLevel);
                    stats.ApplyGrowth();
                }
                return;
            }
        }
    }

    private static Vector3 GetRandomSpawn()
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