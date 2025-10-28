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
        [HideInInspector] public List<GameObject> pool = new List<GameObject>();
    }

    public EnemyPool enemyPool;

    private static EnemySpawner instance;

    void Awake()
    {
        instance = this;
        for (int i = 0; i < enemyPool.poolSize; i++)
        {
            GameObject obj = Instantiate(enemyPool.prefab, transform);
            obj.SetActive(false);
            enemyPool.pool.Add(obj);
        }
    }

    public static void SpawnEnemies(int difficultyLevel)
    {
        if (instance == null)
        {
            Debug.LogError("EnemySpawner not found!");
            return;
        }

        foreach (var enemy in instance.enemyPool.pool)
        {
            if (!enemy.activeInHierarchy)
            {
                enemy.transform.position = GetRandomSpawn();
                enemy.SetActive(true);

                var stats = enemy.GetComponent<Stats>();
                instance.ApplyLevelScaling(stats, difficultyLevel);

                break;
            }
        }
    }

    private void ApplyLevelScaling(Stats stats, int difficulty)
    {
        // Mỗi độ khó sẽ tăng level hoặc scale stats khác nhau
        int levelBoost = difficulty * 2; // ví dụ: easy=0, hard=6

        stats.level = Mathf.Clamp(stats.level + levelBoost, 1, stats.maxLevel);
        stats.currentHP = stats.baseStats.HP * (1 + 0.3f * difficulty);
        stats.currentDMG = stats.baseStats.DMG * (1 + 0.3f * difficulty);
        stats.currentShield = stats.baseStats.Shield * (1 + 0.2f * difficulty);
        stats.currentSPD = stats.baseStats.SPD * (1 + 0.1f * difficulty);
    }

    private static Vector3 GetRandomSpawn()
    {
        return new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
    }
}
