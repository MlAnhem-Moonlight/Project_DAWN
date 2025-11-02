using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyPool
    {
        public string name;
        public GameObject prefab;
        public int poolSize = 10;

        [HideInInspector]
        public List<GameObject> pool = new List<GameObject>();
    }

    [Header("Danh sách các loại Enemy")]
    public List<EnemyPool> enemyPools = new List<EnemyPool>();

    [Header("Điểm spawn (nếu null sẽ random)")]
    public Transform spawnPoint;

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

    public static void SpawnEnemy(int type, int difficultyLevel = 1)
    {
        switch(type)
        {
            case 1:
                SpawnType1(difficultyLevel); 
                break;
            case 2:
                SpawnType2(difficultyLevel);
                break;
            case 3:
                SpawnType3(difficultyLevel);
                break;
            default:
                break;

        }
    }

    // ===============================
    // 🔥 3 kiểu spawn khác nhau
    // ===============================

    /// <summary>
    /// 🧩 Kiểu 1: Spawn 1 enemy duy nhất theo độ khó
    /// </summary>
    [ContextMenu("Spawn Type 1 (Single Enemy)")]
    public static void SpawnType1(int difficultyLevel = 1)
    {
        if (!EnsureInstance()) return;

        var pool = instance.enemyPools[Random.Range(0, instance.enemyPools.Count)];
        SpawnEnemyFromPool(pool, difficultyLevel);
        Debug.Log($"⚔️ SpawnType1: 1 enemy từ {pool.name} với độ khó {difficultyLevel}");
    }

    /// <summary>
    /// 🧟 Kiểu 2: Spawn nhiều enemy cùng loại, số lượng tỉ lệ với độ khó
    /// </summary>
    [ContextMenu("Spawn Type 2 (Scaled Group)")]
    public static void SpawnType2(int difficultyLevel = 1)
    {
        if (!EnsureInstance()) return;

        var pool = instance.enemyPools[Random.Range(0, instance.enemyPools.Count)];
        int count = Mathf.Clamp(difficultyLevel * 2, 2, pool.poolSize); // càng khó càng nhiều

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
                    stats.level = Mathf.Clamp(difficultyLevel, 1, stats.maxLevel);
                    stats.ApplyGrowth();
                }

                spawned++;
                if (spawned >= count) break;
            }
        }

        Debug.Log($"⚔️ SpawnType2: {spawned} enemy từ {pool.name} (difficulty {difficultyLevel})");
    }

    /// <summary>
    /// 🧨 Kiểu 3: Spawn đội hình hỗn hợp — random từ nhiều loại quái
    /// </summary>
    [ContextMenu("Spawn Type 3 (Mixed Group)")]
    public static void SpawnType3(int difficultyLevel = 1)
    {
        if (!EnsureInstance()) return;

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
                        // enemy mạnh hơn nếu độ khó cao, yếu hơn nếu random thấp
                        int scaledLevel = Mathf.Clamp(difficultyLevel + Random.Range(-1, 2), 1, stats.maxLevel);
                        stats.level = scaledLevel;
                        stats.ApplyGrowth();
                    }

                    spawned++;
                    break;
                }
            }
        }

        Debug.Log($"⚔️ SpawnType3: {spawned} enemy hỗn hợp với độ khó {difficultyLevel}");
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

    private static void SpawnEnemyFromPool(EnemyPool pool, int difficultyLevel)
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
                    stats.level = Mathf.Clamp(difficultyLevel, 1, stats.maxLevel);
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
}
