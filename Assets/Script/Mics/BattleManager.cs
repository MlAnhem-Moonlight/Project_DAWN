using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    public BattleStateCollector stateCollector;
    public QLearningEnemyBalancer qLearning;
    public EnemySpawner enemySpawner;  // ✅ Existing reference

    [Header("Spawn Configuration")]
    [Tooltip("Loại spawn: 1 = Single, 2 = Scaled Group, 3 = Mixed")]
    public int spawnType = 1;

    [Header("Combat Session Configuration")]
    [Tooltip("Thời gian chiến đấu tối thiểu (giây)")]
    public float minCombatDuration = 90f;

    [Tooltip("Thời gian chiến đấu tối đa (giây)")]
    public float maxCombatDuration = 180f;

    [Tooltip("Delay giữa các wave (giây)")]
    public float waveDelay = 2f;

    [Header("Battle State")]
    private float combatSessionStartTime;
    private float totalCombatTime = 0f;
    private int currentWave = 0;
    private bool combatSessionActive = false;

    private bool battleStarted = false;
    private bool enemiesSpawned = false;
    private bool enemiesFullyActivated = false;
    private bool waveEnded = false;

    private string currentWaveState;
    private int currentWaveDifficulty;
    private int currentWaveEnemyLevel;
    private float currentWaveStartTime;

    [Header("Wave Tracking")]
    private System.Collections.Generic.List<WaveData> waveHistory = new System.Collections.Generic.List<WaveData>();

    [Header("Win Condition Tracking")]
    public int aliveEnemyCount = 0;
    public int aliveAllyCount = 0;

    [Header("Balance Metrics")]
    [Tooltip("Thời gian trận đấu lý tưởng cho mỗi wave (giây)")]
    public float idealWaveDuration = 25f;

    [Tooltip("Khoảng thời gian chấp nhận được")]
    public float durationTolerance = 10f;


    [System.Serializable]
    private class WaveData
    {
        public int waveNumber;
        public string allyState;
        public int difficulty;
        public int enemyLevel;
        public float duration;
        public bool allyWon;
        public int aliveAllies;
        public int aliveEnemies;
        public float reward;
    }

    void Start()
    {
        qLearning.LoadQTable();

        if (qLearning.stateCollector == null)
        {
            qLearning.stateCollector = stateCollector;
        }
    }

    IEnumerator WaitForAllyAndStartCombatSession()
    {
        Debug.Log("⏳ Đang chờ ally xuất hiện...");

        yield return new WaitUntil(() =>
            stateCollector != null &&
            stateCollector.activeAllies.Count > 0 &&
            !string.IsNullOrEmpty(stateCollector.currentState)
        );

        Debug.Log($"✅ Phát hiện {stateCollector.activeAllies.Count} ally. Bắt đầu Combat Session!");
        yield return new WaitForSeconds(0.5f);

        StartCombatSession();
    }

    [ContextMenu("⚔️ Start Combat Session")]
    public void StartCombatSession()
    {
        if (combatSessionActive)
        {
            Debug.LogWarning("⚠️ Combat Session đã đang chạy!");
            return;
        }

        if (stateCollector == null || qLearning == null)
        {
            Debug.LogError("❌ StateCollector hoặc QLearning chưa được gán!");
            return;
        }

        if (stateCollector.activeAllies.Count == 0)
        {
            Debug.LogWarning("⚠️ Không có ally nào trên sân!");
            return;
        }

        combatSessionActive = true;
        combatSessionStartTime = Time.time;
        totalCombatTime = 0f;
        currentWave = 0;
        waveHistory.Clear();

        Debug.Log($"🎮 === COMBAT SESSION BẮT ĐẦU ===\n" +
                  $"  🎚️ Player Level: {stateCollector.playerLevel}\n" +
                  $"  ⏱️ Mục tiêu: {minCombatDuration}s - {maxCombatDuration}s\n" +
                  $"  👥 Ally ban đầu: {stateCollector.activeAllies.Count}\n" +
                  $"  📊 Power Ratio: {stateCollector.GetPowerToLevelRatio():F2}");

        StartNextWave();
    }

    void StartNextWave()
    {
        if (!combatSessionActive) return;

        CountLivingUnits();
        if (aliveAllyCount == 0)
        {
            Debug.Log("💀 Tất cả ally đã bị tiêu diệt. Kết thúc Combat Session.");
            EndCombatSession();
            return;
        }

        currentWave++;
        currentWaveState = stateCollector.GetSimpleState();

        currentWaveDifficulty = qLearning.ChooseEnemySetup(currentWaveState);
        currentWaveEnemyLevel = qLearning.GetCurrentEnemyLevel();

        currentWaveStartTime = Time.time;
        waveEnded = false;

        Debug.Log($"\n🌊 === WAVE {currentWave} ===\n" +
                  $"  🎚️ Player Level: {stateCollector.playerLevel}\n" +
                  $"  📊 Ally State: {currentWaveState}\n" +
                  $"  🎯 Difficulty: {currentWaveDifficulty} (số lượng/composition)\n" +
                  $"  ⚡ Enemy Level: {currentWaveEnemyLevel}\n" +
                  $"  👥 Ally còn lại: {aliveAllyCount}\n" +
                  $"  ⏱️ Tổng thời gian combat: {totalCombatTime:F1}s / {minCombatDuration}s");

        StartCoroutine(SpawnAndWaitForEnemies());
    }

    IEnumerator SpawnAndWaitForEnemies()
    {
        enemiesSpawned = true;

        enemySpawner.SpawnEnemy(spawnType, currentWaveDifficulty, currentWaveEnemyLevel);

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);

        CountLivingUnits();

        if (aliveEnemyCount == 0)
        {
            Debug.LogWarning("⚠️ Không có enemy nào được spawn! Đợi thêm...");
            yield return new WaitForSeconds(0.5f);
            CountLivingUnits();
        }

        enemiesFullyActivated = true;
        battleStarted = true;

        Debug.Log($"✅ Wave {currentWave} đã spawn xong! Enemy: {aliveEnemyCount} (Level {currentWaveEnemyLevel})");
    }

    void OnWaveEnd(bool allyWon)
    {
        if (!battleStarted || waveEnded) return;

        waveEnded = true;

        float waveDuration = Time.time - currentWaveStartTime;
        totalCombatTime += waveDuration;

        CountLivingUnits();

        float reward = CalculateBalancedReward(allyWon, waveDuration);

        WaveData waveData = new WaveData
        {
            waveNumber = currentWave,
            allyState = currentWaveState,
            difficulty = currentWaveDifficulty,
            enemyLevel = currentWaveEnemyLevel,
            duration = waveDuration,
            allyWon = allyWon,
            aliveAllies = aliveAllyCount,
            aliveEnemies = aliveEnemyCount,
            reward = reward
        };
        waveHistory.Add(waveData);

        Debug.Log($"🏁 === WAVE {currentWave} KẾT THÚC ===\n" +
                  $"  🏆 Người thắng: {(allyWon ? "ALLY" : "ENEMY")}\n" +
                  $"  ⏱️ Thời gian wave: {waveDuration:F1}s\n" +
                  $"  ⏱️ Tổng combat: {totalCombatTime:F1}s / {minCombatDuration}s\n" +
                  $"  💰 Reward: {reward:F2}\n" +
                  $"  👥 Còn lại - Ally: {aliveAllyCount}, Enemy: {aliveEnemyCount}\n" +
                  $"  ⚡ Enemy Level: {currentWaveEnemyLevel}");

        string nextState = allyWon ? stateCollector.GetSimpleState() : "Defeated";
        qLearning.UpdateAfterBattle(nextState, reward, allyWon, waveDuration);

        if (!allyWon)
        {
            qLearning.DisableAllEnemies();
        }

        battleStarted = false;
        enemiesSpawned = false;
        enemiesFullyActivated = false;

        if (!allyWon || aliveAllyCount == 0)
        {
            EndCombatSession();
            return;
        }

        if (totalCombatTime >= minCombatDuration)
        {
            Debug.Log($"✅ Đã đạt thời gian tối thiểu ({totalCombatTime:F1}s ≥ {minCombatDuration}s). Kết thúc Combat Session.");
            EndCombatSession();
            return;
        }

        if (totalCombatTime >= maxCombatDuration)
        {
            Debug.Log($"⏰ Đã vượt thời gian tối đa ({totalCombatTime:F1}s ≥ {maxCombatDuration}s). Kết thúc Combat Session.");
            EndCombatSession();
            return;
        }

        Debug.Log($"⏳ Chưa đủ thời gian ({totalCombatTime:F1}s < {minCombatDuration}s). Tiếp tục wave tiếp theo...");
        StartCoroutine(PrepareNextWave());
    }

    IEnumerator PrepareNextWave()
    {
        Debug.Log($"⏳ Chuẩn bị wave tiếp theo sau {waveDelay}s...");
        yield return new WaitForSeconds(waveDelay);

        stateCollector.RefreshActiveAllies();

        StartNextWave();
    }

    void EndCombatSession()
    {
        combatSessionActive = false;

        Debug.Log($"\n🎊 === COMBAT SESSION KẾT THÚC ===\n" +
                  $"  🌊 Tổng số wave: {currentWave}\n" +
                  $"  ⏱️ Tổng thời gian combat: {totalCombatTime:F1}s\n" +
                  $"  👥 Ally còn lại: {aliveAllyCount}\n" +
                  $"  📈 Wave history:");
        Debug.Log($"  Reward tổng: {CalculateTotalReward()}");
        foreach (var wave in waveHistory)
        {
            Debug.Log($"    Wave {wave.waveNumber}: Lv{wave.enemyLevel} | {wave.duration:F1}s | " +
                     $"{(wave.allyWon ? "WIN" : "LOSE")} | " +
                     $"Reward: {wave.reward:F2} | " +
                     $"Difficulty: {wave.difficulty}");
        }

        // ✅ Lưu tài nguyên đã sử dụng
        SaveDayResources();

        qLearning.SaveQTable();
        GameController gameController = FindAnyObjectByType<GameController>();
        ResourceAllocationCMAES CMAES = FindAnyObjectByType<ResourceAllocationCMAES>();
        if (gameController != null && CMAES != null)
        {
            gameController.GameStateController();
            CMAES.RunCMAES();
        }
    }

    // ✅ Lưu tài nguyên đã sử dụng trong ngày
    void SaveDayResources()
    {
        IngridientManager ingredientManager = FindAnyObjectByType<IngridientManager>();
        ResourceSpawnPredictor predictor = FindAnyObjectByType<ResourceSpawnPredictor>();

        if (ingredientManager == null || predictor == null) return;

        // Lấy dữ liệu tài nguyên hiện tại
        ResourceData remainingResources = ingredientManager.GetResourceData();
        ResourceData consumedResources = ingredientManager.GetConsumedResourceData();

        Debug.Log($"💾 === SAVING DAY RESOURCES ===\n" +
                  $"  Remaining: Wood={remainingResources.wood}, Stone={remainingResources.stone}, " +
                  $"Iron={remainingResources.iron}, Gold={remainingResources.gold}, Meat={remainingResources.meat}\n" +
                  $"  Consumed: Wood={consumedResources.wood}, Stone={consumedResources.stone}, " +
                  $"Iron={consumedResources.iron}, Gold={consumedResources.gold}, Meat={consumedResources.meat}");

        // ✅ Cập nhật predictor với dữ liệu tài nguyên thực tế
        predictor.UpdateTestData(remainingResources, consumedResources);

        // ✅ Thêm vào training data (để predictor học từ ngày này)
        int playerLevel = FindAnyObjectByType<BattleStateCollector>().playerLevel;
        predictor.AddTrainingData(playerLevel, remainingResources, consumedResources, remainingResources);

        // ✅ Retrain model để cải thiện dự đoán
        predictor.RetrainModel();

        Debug.Log("✅ Predictor updated with today's resource data");
    }

    // ✅ Reset consumed resources cho ngày mới
    public void ResetDayResources()
    {
        IngridientManager ingredientManager = FindAnyObjectByType<IngridientManager>();
        if (ingredientManager != null)
        {
            ingredientManager.ResetConsumedResources();
            Debug.Log("🔄 Day resources reset for new day");
        }
    }

    float CalculateBalancedReward(bool allyWon, float waveDuration)
    {
        float reward = 0f;

        float durationDeviation = Mathf.Abs(waveDuration - idealWaveDuration);

        if (durationDeviation <= durationTolerance)
        {
            reward += 15f;
            Debug.Log($"⏱️ Thời gian tốt ({waveDuration:F1}s ≈ {idealWaveDuration}s) → +15");
        }
        else if (waveDuration < 10f)
        {
            reward -= 15f;
            Debug.Log($"⚡ Kết thúc quá nhanh ({waveDuration:F1}s) → -15");
        }
        else if (waveDuration > 45f)
        {
            reward -= 10f;
            Debug.Log($"🐌 Wave quá dài ({waveDuration:F1}s) → -10");
        }

        if (allyWon)
        {
            if (aliveAllyCount <= 2 && waveDuration > 20f)
            {
                reward += 10f;
                Debug.Log($"⚖️ Ally thắng sát sao (còn {aliveAllyCount} quân) → +10");
            }
            else if (aliveAllyCount >= 4 || waveDuration < 15f)
            {
                reward -= 10f;
                Debug.Log($"😴 Ally thắng dễ (còn {aliveAllyCount} quân) → -10");
            }
        }
        else
        {
            if (aliveEnemyCount <= 2 && waveDuration > 20f)
            {
                reward += 10f;
                Debug.Log($"⚖️ Enemy thắng sát sao (còn {aliveEnemyCount} quân) → +10");
            }
            else if (aliveEnemyCount >= 4 || waveDuration < 15f)
            {
                reward -= 10f;
                Debug.Log($"💪 Enemy thắng dễ (còn {aliveEnemyCount} quân) → -10");
            }
        }

        float finalHealthRatio = (float)aliveAllyCount / Mathf.Max(1, aliveAllyCount + aliveEnemyCount);
        if (finalHealthRatio > 0.3f && finalHealthRatio < 0.7f)
        {
            reward += 5f;
            Debug.Log($"🎭 Trận đấu căng thẳng (health ratio: {finalHealthRatio:F2}) → +5");
        }

        int levelDiff = Mathf.Abs(currentWaveEnemyLevel - stateCollector.playerLevel);
        if (levelDiff <= 2)
        {
            reward += 5f;
            Debug.Log($"🎚️ Enemy level phù hợp (diff: {levelDiff}) → +5");
        }

        return reward;
    }

    // ✅ METHOD MỚI: Đếm enemy từ pool thay vì dùng FindGameObjectsWithTag
    void CountLivingUnits()
    {
        // ✅ Đếm ally từ tag (như cũ)
        var allAllies = GameObject.FindGameObjectsWithTag("Ally");
        aliveAllyCount = allAllies.Count(obj =>
            obj.activeInHierarchy &&
            obj.GetComponent<Stats>() != null &&
            obj.GetComponent<Stats>().currentHP > 0
        );

        // ✅ Đếm enemy từ pool của EnemySpawner (chính xác hơn)
        aliveEnemyCount = CountEnemyFromPool();

        //Debug.Log($"📊 Unit Count - Ally: {aliveAllyCount}, Enemy: {aliveEnemyCount}");
    }

    // ✅ Hàm mới: Đếm enemy còn sống trong pool
    int CountEnemyFromPool()
    {
        if (enemySpawner == null)
        {
            Debug.LogWarning("⚠️ EnemySpawner chưa được gán!");
            return 0;
        }

        int count = 0;

        // ✅ Lặp qua tất cả pool (enemy và ally)
        foreach (var pool in enemySpawner.enemyPools)
        {
            // ✅ Chỉ đếm Enemy pool, bỏ qua Ally pool
            if (pool.faction != UnitFaction.Enemy) continue;

            // ✅ Đếm những enemy đang active và còn HP > 0
            foreach (var enemy in pool.pool)
            {
                if (enemy == null) continue;

                if (!enemy.activeInHierarchy) continue;

                Stats stats = enemy.GetComponent<Stats>();
                if (stats != null && stats.currentHP > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    void Update()
    {
        if (!battleStarted || !enemiesFullyActivated) return;

        CountLivingUnits();

        if (!waveEnded)
        {
            if (aliveEnemyCount == 0 && aliveAllyCount > 0)
            {
                Debug.Log("✅ ALLY WIN!");
                OnWaveEnd(allyWon: true);
            }
            else if (aliveAllyCount == 0 && aliveEnemyCount > 0)
            {
                Debug.Log("❌ ALLY LOSE!");
                OnWaveEnd(allyWon: false);
            }
        }
    }

    [ContextMenu("🔄 Reset Combat Session")]
    public void ResetCombatSession()
    {
        combatSessionActive = false;
        battleStarted = false;
        enemiesSpawned = false;
        enemiesFullyActivated = false;
        waveEnded = false;
        totalCombatTime = 0f;
        currentWave = 0;
        waveHistory.Clear();

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.SetActive(false);
        }

        Debug.Log("🔄 Combat Session đã được reset");
    }

    [ContextMenu("📊 Print Session Stats")]
    public void PrintSessionStats()
    {
        Debug.Log($"=== COMBAT SESSION STATS ===\n" +
                  $"Active: {combatSessionActive}\n" +
                  $"Player Level: {stateCollector.playerLevel}\n" +
                  $"Current Wave: {currentWave}\n" +
                  $"Total Combat Time: {totalCombatTime:F1}s\n" +
                  $"Target: {minCombatDuration}s - {maxCombatDuration}s\n" +
                  $"Waves Completed: {waveHistory.Count}");
    }

    [ContextMenu("🎚️ Set Player Level")]
    public void SetPlayerLevel()
    {
        // Gọi menu này để test với level khác nhau
    }

    // ✅ Method mới: Tính tổng reward từ tất cả wave
    float CalculateTotalReward()
    {
        float totalReward = 0f;

        foreach (var wave in waveHistory)
        {
            totalReward += wave.reward;
        }

        return totalReward;
    }
}