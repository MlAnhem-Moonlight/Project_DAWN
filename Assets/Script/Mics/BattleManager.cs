using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("References")]
    public BattleStateCollector stateCollector;
    public QLearningEnemyBalancer qLearning;

    [Header("Spawn Configuration")]
    [Tooltip("Loại spawn: 1 = Single, 2 = Scaled Group, 3 = Mixed")]
    public int spawnType = 1;

    [Header("Combat Session Configuration")]
    [Tooltip("Thời gian chiến đấu tối thiểu (giây)")]
    public float minCombatDuration = 90f; // 1p30s

    [Tooltip("Thời gian chiến đấu tối đa (giây)")]
    public float maxCombatDuration = 180f; // 3p

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

    private string currentWaveState;
    private int currentWaveDifficulty;
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
        public float duration;
        public bool allyWon;
        public int aliveAllies;
        public int aliveEnemies;
        public float reward;
    }

    void Start()
    {
        qLearning.LoadQTable();
        //StartCoroutine(WaitForAllyAndStartCombatSession());
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
                  $"  ⏱️ Mục tiêu: {minCombatDuration}s - {maxCombatDuration}s\n" +
                  $"  👥 Ally ban đầu: {stateCollector.activeAllies.Count}");

        StartNextWave();
    }

    void StartNextWave()
    {
        if (!combatSessionActive) return;

        // Kiểm tra nếu đã đủ thời gian tối thiểu
        if (totalCombatTime >= minCombatDuration)
        {
            Debug.Log($"✅ Đã đạt thời gian tối thiểu ({totalCombatTime:F1}s). Kết thúc Combat Session.");
            EndCombatSession();
            return;
        }

        // Kiểm tra nếu vượt quá thời gian tối đa
        if (totalCombatTime >= maxCombatDuration)
        {
            Debug.Log($"⏰ Đã vượt thời gian tối đa ({totalCombatTime:F1}s). Kết thúc Combat Session.");
            EndCombatSession();
            return;
        }

        // Kiểm tra ally còn sống
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
        currentWaveStartTime = Time.time;

        Debug.Log($"\n🌊 === WAVE {currentWave} ===\n" +
                  $"  📊 Ally State: {currentWaveState}\n" +
                  $"  🎯 Difficulty: {currentWaveDifficulty}\n" +
                  $"  👥 Ally còn lại: {aliveAllyCount}\n" +
                  $"  ⏱️ Tổng thời gian combat: {totalCombatTime:F1}s / {minCombatDuration}s");

        StartCoroutine(SpawnAndWaitForEnemies());
    }

    IEnumerator SpawnAndWaitForEnemies()
    {
        enemiesSpawned = true;
        EnemySpawner.SpawnEnemy(spawnType, currentWaveDifficulty);

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

        Debug.Log($"✅ Wave {currentWave} đã spawn xong! Enemy: {aliveEnemyCount}");
    }

    void OnWaveEnd(bool allyWon)
    {
        if (!battleStarted) return;

        float waveDuration = Time.time - currentWaveStartTime;
        totalCombatTime += waveDuration;

        CountLivingUnits();

        float reward = CalculateBalancedReward(allyWon, waveDuration);

        // Lưu wave data
        WaveData waveData = new WaveData
        {
            waveNumber = currentWave,
            allyState = currentWaveState,
            difficulty = currentWaveDifficulty,
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
                  $"  👥 Còn lại - Ally: {aliveAllyCount}, Enemy: {aliveEnemyCount}");

        // Update Q-Learning
        string nextState = allyWon ? stateCollector.GetSimpleState() : "Defeated";
        qLearning.UpdateAfterBattle(nextState, reward, allyWon, waveDuration);

        // Disable enemies
        if (!allyWon)
        {
            qLearning.DisableAllEnemies();
        }

        // Reset wave flags
        battleStarted = false;
        enemiesSpawned = false;
        enemiesFullyActivated = false;

        // Nếu ally thua, kết thúc session
        if (!allyWon)
        {
            EndCombatSession();
            return;
        }

        // Nếu ally thắng, chờ và spawn wave tiếp
        StartCoroutine(PrepareNextWave());
    }

    IEnumerator PrepareNextWave()
    {
        Debug.Log($"⏳ Chuẩn bị wave tiếp theo sau {waveDelay}s...");
        yield return new WaitForSeconds(waveDelay);

        // Refresh ally state
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

        foreach (var wave in waveHistory)
        {
            Debug.Log($"    Wave {wave.waveNumber}: {wave.duration:F1}s | " +
                     $"{(wave.allyWon ? "WIN" : "LOSE")} | " +
                     $"Reward: {wave.reward:F2} | " +
                     $"Difficulty: {wave.difficulty}");
        }

        // Save Q-Table
        qLearning.SaveQTable();
    }

    float CalculateBalancedReward(bool allyWon, float waveDuration)
    {
        float reward = 0f;

        // 1️⃣ Đánh giá thời gian wave
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

        // 2️⃣ Đánh giá tình trạng chiến thắng
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

        // 3️⃣ Bonus cho trận đấu căng thẳng
        float finalHealthRatio = (float)aliveAllyCount / Mathf.Max(1, aliveAllyCount + aliveEnemyCount);
        if (finalHealthRatio > 0.3f && finalHealthRatio < 0.7f)
        {
            reward += 5f;
            Debug.Log($"🎭 Trận đấu căng thẳng (health ratio: {finalHealthRatio:F2}) → +5");
        }

        return reward;
    }

    void CountLivingUnits()
    {
        var allAllies = GameObject.FindGameObjectsWithTag("Ally");
        aliveAllyCount = allAllies.Count(obj =>
            obj.activeInHierarchy &&
            obj.GetComponent<Stats>() != null &&
            obj.GetComponent<Stats>().currentHP > 0
        );

        var allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        aliveEnemyCount = allEnemies.Count(obj =>
            obj.activeInHierarchy &&
            obj.GetComponent<Stats>() != null &&
            obj.GetComponent<Stats>().currentHP > 0
        );
    }

    void Update()
    {
        if (!battleStarted || !enemiesFullyActivated) return;

        CountLivingUnits();

        if (aliveEnemyCount == 0 && aliveAllyCount > 0)
        {
            OnWaveEnd(allyWon: true);
        }
        else if (aliveAllyCount == 0 && aliveEnemyCount > 0)
        {
            OnWaveEnd(allyWon: false);
        }
    }

    [ContextMenu("🔄 Reset Combat Session")]
    public void ResetCombatSession()
    {
        combatSessionActive = false;
        battleStarted = false;
        enemiesSpawned = false;
        enemiesFullyActivated = false;
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
                  $"Current Wave: {currentWave}\n" +
                  $"Total Combat Time: {totalCombatTime:F1}s\n" +
                  $"Target: {minCombatDuration}s - {maxCombatDuration}s\n" +
                  $"Waves Completed: {waveHistory.Count}");
    }
}