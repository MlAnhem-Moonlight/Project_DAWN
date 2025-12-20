using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SerializableQTable
{
    public List<string> keys = new List<string>();
    public List<FloatArrayWrapper> values = new List<FloatArrayWrapper>(); // ✅ Wrapper để serialize array

    [System.Serializable]
    public class FloatArrayWrapper
    {
        public float[] data = new float[0];

        public FloatArrayWrapper() { }
        public FloatArrayWrapper(float[] arr) => data = arr;
    }

    public SerializableQTable() { } // ✅ Constructor mặc định cho deserialization

    public SerializableQTable(Dictionary<string, float[]> qTable)
    {
        foreach (var kvp in qTable)
        {
            keys.Add(kvp.Key);
            values.Add(new FloatArrayWrapper(kvp.Value));
        }
    }

    public Dictionary<string, float[]> ToDictionary()
    {
        Dictionary<string, float[]> dict = new Dictionary<string, float[]>();
        for (int i = 0; i < keys.Count; i++)
        {
            if (values[i]?.data != null)
                dict[keys[i]] = values[i].data;
        }
        return dict;
    }
}

public class QLearningEnemyBalancer : MonoBehaviour
{
    [Header("Q-Learning Parameters")]
    public int actionCount = 4;
    public float learningRate = 0.1f;
    public float discountFactor = 0.9f;
    public float explorationRate = 0.2f;

    [Header("Adaptive Difficulty")]
    [Tooltip("Nếu true, sẽ điều chỉnh độ khó dựa trên kết quả trận trước")]
    public bool enableAdaptiveDifficulty = true;

    [Tooltip("Lưu kết quả trận trước để điều chỉnh")]
    private bool lastBattleAllyWon = false;
    private int consecutiveAllyWins = 0;
    private int consecutiveEnemyWins = 0;

    [Header("Level-Based Spawning")]
    [Tooltip("Reference đến StateCollector để lấy player level")]
    public BattleStateCollector stateCollector;

    [Header("Debug")]
    [Tooltip("Bật để xem chi tiết save/load")]
    public bool verboseLogging = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, "qtable.json");
    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();

    private string lastState;
    private int lastAction;
    private bool hasUnsavedChanges = false;

    // Lưu enemy level cho wave hiện tại
    private int currentEnemyLevel = 1;

    void OnApplicationQuit()
    {
        if (hasUnsavedChanges)
        {
            Debug.Log("💾 Auto-saving Q-Table before quit...");
            SaveQTable();
        }
    }

    /// <summary>
    /// Chọn setup enemy: trả về difficulty (số lượng/composition)
    /// Level của enemy sẽ được xác định riêng bởi player level
    /// </summary>
    public int ChooseEnemySetup(string state)
    {
        // Khởi tạo state mới nếu chưa tồn tại
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[actionCount];
            for (int i = 0; i < actionCount; i++)
                qTable[state][i] = 0f;

            hasUnsavedChanges = true;

            if (verboseLogging)
                Debug.Log($"🆕 Created new state: {state}");
        }

        int action;

        if (Random.value < explorationRate)
        {
            action = Random.Range(0, actionCount);
            Debug.Log($"🧠 Explore random action: {action}");
        }
        else
        {
            action = ArgMax(qTable[state]);

            if (enableAdaptiveDifficulty)
            {
                action = ApplyAdaptiveAdjustment(action);
            }

            Debug.Log($"🎯 Exploit best action: {action} (Q={qTable[state][action]:F2})");
        }

        lastState = state;
        lastAction = action;

        // Xác định enemy level dựa trên player level
        if (stateCollector != null)
        {
            currentEnemyLevel = stateCollector.GetRecommendedEnemyLevel();

            // Điều chỉnh dựa trên power ratio
            float powerRatio = stateCollector.GetPowerToLevelRatio();

            if (powerRatio < 0.7f)
            {
                // Player yếu so với level → giảm enemy level xuống 1-2 level
                currentEnemyLevel = Mathf.Max(1, currentEnemyLevel - Random.Range(1, 3));
                Debug.Log($"📉 Player yếu (ratio={powerRatio:F2}) → Enemy level: {currentEnemyLevel}");
            }
            else if (powerRatio > 1.3f)
            {
                // Player mạnh so với level → tăng enemy level lên 1-2 level
                currentEnemyLevel = Mathf.Min(20, currentEnemyLevel + Random.Range(1, 3));
                Debug.Log($"📈 Player mạnh (ratio={powerRatio:F2}) → Enemy level: {currentEnemyLevel}");
            }
            else
            {
                Debug.Log($"⚖️ Player cân bằng (ratio={powerRatio:F2}) → Enemy level: {currentEnemyLevel}");
            }
        }

        return action;
    }

    /// <summary>
    /// Trả về level của enemy cho wave hiện tại
    /// </summary>
    public int GetCurrentEnemyLevel()
    {
        return currentEnemyLevel;
    }

    private int ApplyAdaptiveAdjustment(int baseAction)
    {
        int adjustedAction = baseAction;

        // Điều chỉnh số lượng enemy (action), KHÔNG điều chỉnh level
        if (consecutiveAllyWins >= 2)
        {
            // Tăng số lượng enemy
            adjustedAction = Mathf.Min(actionCount - 1, baseAction + 1);
            Debug.Log($"📈 Tăng số lượng enemy: {baseAction} → {adjustedAction} (Ally thắng {consecutiveAllyWins} trận liên tiếp)");
        }
        else if (consecutiveAllyWins >= 1)
        {
            if (Random.value < 0.5f)
                adjustedAction = Mathf.Min(actionCount - 1, baseAction + 1);
        }

        if (consecutiveEnemyWins >= 2)
        {
            // Giảm số lượng enemy
            adjustedAction = Mathf.Max(0, baseAction - 1);
            Debug.Log($"📉 Giảm số lượng enemy: {baseAction} → {adjustedAction} (Enemy thắng {consecutiveEnemyWins} trận liên tiếp)");
        }
        else if (consecutiveEnemyWins >= 1)
        {
            if (Random.value < 0.5f)
                adjustedAction = Mathf.Max(0, baseAction - 1);
        }

        return adjustedAction;
    }

    public void UpdateAfterBattle(string nextState, float reward, bool allyWon, float battleDuration)
    {
        if (string.IsNullOrEmpty(lastState))
        {
            Debug.LogWarning("⚠️ No previous state recorded!");
            return;
        }

        // Update consecutive wins
        if (allyWon)
        {
            consecutiveAllyWins++;
            consecutiveEnemyWins = 0;
        }
        else
        {
            consecutiveEnemyWins++;
            consecutiveAllyWins = 0;
        }

        lastBattleAllyWon = allyWon;

        // Khởi tạo states nếu chưa tồn tại
        if (!qTable.ContainsKey(lastState))
        {
            qTable[lastState] = new float[actionCount];
            for (int i = 0; i < actionCount; i++)
                qTable[lastState][i] = 0f;
        }

        if (!qTable.ContainsKey(nextState))
        {
            qTable[nextState] = new float[actionCount];
            for (int i = 0; i < actionCount; i++)
                qTable[nextState][i] = 0f;
        }

        // Q-Learning update
        float oldValue = qTable[lastState][lastAction];
        float maxNextQ = qTable[nextState].Max();
        float newValue = oldValue + learningRate * (reward + discountFactor * maxNextQ - oldValue);

        qTable[lastState][lastAction] = newValue;

        hasUnsavedChanges = true;

        if (verboseLogging)
        {
            Debug.Log($"✅ Q-Update:\n" +
                     $"  State: {lastState}\n" +
                     $"  Action: {lastAction}\n" +
                     $"  Enemy Level: {currentEnemyLevel}\n" +
                     $"  Old: {oldValue:F3} → New: {newValue:F3}\n" +
                     $"  Reward: {reward:F2}, MaxNextQ: {maxNextQ:F2}\n" +
                     $"  Q-Array: [{string.Join(", ", qTable[lastState].Select(v => v.ToString("F2")))}]");
        }

        // Auto-save sau mỗi 5 updates
        if (hasUnsavedChanges && qTable.Count % 5 == 0)
        {
            SaveQTable();
        }
    }

    [ContextMenu("Disable All Enemies")]
    public void DisableAllEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int count = 0;

        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                enemy.SetActive(false);
                count++;
            }
        }

        Debug.Log($"🛑 Đã tắt toàn bộ enemy ({count} đối tượng).");
    }

    [ContextMenu("Save Q-Table")]
    public void SaveQTable()
    {
        try
        {
            if (qTable == null || qTable.Count == 0)
            {
                Debug.LogWarning("⚠️ Q-Table rỗng, không save.");
                return;
            }

            // Verify integrity trước khi save
            foreach (var kvp in qTable)
            {
                if (kvp.Value == null || kvp.Value.Length != actionCount)
                {
                    Debug.LogError($"❌ Invalid entry for state '{kvp.Key}': array is null or wrong size");
                    return;
                }
            }

            SerializableQTable s = new SerializableQTable(qTable);

            if (s.keys.Count != s.values.Count)
            {
                Debug.LogError($"❌ Serialize error: keys={s.keys.Count}, values={s.values.Count}");
                return;
            }

            string json = JsonUtility.ToJson(s, true);

            if (string.IsNullOrEmpty(json) || json.Length < 10)
            {
                Debug.LogError($"❌ Generated JSON is invalid: length={json?.Length}");
                return;
            }

            File.WriteAllText(SavePath, json);
            hasUnsavedChanges = false;

            Debug.Log($"✅ Saved Q-Table: {SavePath}\n" +
                     $"  Entries: {qTable.Count}\n" +
                     $"  File size: {new FileInfo(SavePath).Length} bytes");

            if (verboseLogging && qTable.Count > 0)
            {
                var sample = qTable.First();
                Debug.Log($"📝 Sample: {sample.Key} → [{string.Join(", ", sample.Value.Select(v => v.ToString("F2")))}]");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Save failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [ContextMenu("Load Q-Table")]
    public void LoadQTable()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"⚠️ Q-Table not found at {SavePath}. Starting fresh.");
                qTable = new Dictionary<string, float[]>();
                return;
            }

            string json = File.ReadAllText(SavePath);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("⚠️ Q-Table file empty. Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            // ✅ Thêm validation chi tiết trước deserialize
            if (!json.Contains("\"keys\"") || !json.Contains("\"values\""))
            {
                Debug.LogError("❌ JSON missing required fields (keys/values). Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            SerializableQTable s = JsonUtility.FromJson<SerializableQTable>(json);

            if (s == null)
            {
                Debug.LogError("❌ Failed to deserialize (returned null). File format may be corrupted. Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            if (s.keys == null || s.values == null)
            {
                Debug.LogError("❌ Deserialized object has null collections. Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            if (s.keys.Count != s.values.Count)
            {
                Debug.LogError($"❌ Mismatch: keys={s.keys.Count}, values={s.values.Count}. Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            // ✅ Validate từng entry
            for (int i = 0; i < s.values.Count; i++)
            {
                if (s.values[i] == null || s.values[i].data == null)
                {
                    Debug.LogError($"❌ Entry {i} ({s.keys[i]}) has null data wrapper. Resetting.");
                    qTable = new Dictionary<string, float[]>();
                    File.Delete(SavePath);
                    return;
                }

                if (s.values[i].data.Length != actionCount)
                {
                    Debug.LogWarning($"⚠️ Entry {i} ({s.keys[i]}) has mismatched array size: {s.values[i].data.Length} (expected {actionCount}). Skipping entry.");
                    // Nếu muốn reset toàn bộ:
                    // qTable = new Dictionary<string, float[]>();
                    // File.Delete(SavePath);
                    // return;
                    continue; // ✅ Skip entry thay vì reset toàn bộ
                }
            }

            qTable = s.ToDictionary();
            hasUnsavedChanges = false;

            Debug.Log($"📂 Loaded Q-Table: {SavePath}\n" +
                     $"  Entries: {qTable.Count}\n" +
                     $"  File size: {new FileInfo(SavePath).Length} bytes");

            if (verboseLogging && qTable.Count > 0)
            {
                var sample = qTable.First();
                //Debug.Log($"📝 Sample: {sample.Key} → [{string.Join(", ", sample.Value.Select(v => v.ToString("F2")))}]");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Load failed: {ex.Message}\n{ex.StackTrace}");
            qTable = new Dictionary<string, float[]>();
            try { if (File.Exists(SavePath)) File.Delete(SavePath); } catch { }
        }
    }

    [ContextMenu("Print Q-Table")]
    public void DebugQTable()
    {
        Debug.Log($"=== Q-TABLE ({qTable.Count} states) ===");

        if (qTable.Count == 0)
        {
            Debug.Log("❌ Q-Table is empty!");
            return;
        }

        foreach (var kvp in qTable)
        {
            string values = string.Join(", ", kvp.Value.Select(v => v.ToString("F2")));
            int bestAction = ArgMax(kvp.Value);
            float bestValue = kvp.Value[bestAction];
            Debug.Log($"State: {kvp.Key}\n" +
                     $"  Q: [{values}]\n" +
                     $"  Best: Action {bestAction} (Q={bestValue:F2})");
        }

        Debug.Log($"\nStats:\n" +
                 $"  Consecutive - Ally: {consecutiveAllyWins}, Enemy: {consecutiveEnemyWins}\n" +
                 $"  Unsaved changes: {hasUnsavedChanges}");
    }

    [ContextMenu("Reset Adaptive Stats")]
    public void ResetAdaptiveStats()
    {
        consecutiveAllyWins = 0;
        consecutiveEnemyWins = 0;
        Debug.Log("🔄 Reset adaptive difficulty tracking");
    }

    [ContextMenu("Verify Q-Table Integrity")]
    public void VerifyIntegrity()
    {
        Debug.Log($"🔍 Verifying Q-Table integrity...");

        bool isValid = true;
        int invalidCount = 0;

        foreach (var kvp in qTable)
        {
            if (kvp.Value == null)
            {
                Debug.LogError($"❌ State '{kvp.Key}' has null array!");
                isValid = false;
                invalidCount++;
            }
            else if (kvp.Value.Length != actionCount)
            {
                Debug.LogError($"❌ State '{kvp.Key}' has wrong array size: {kvp.Value.Length} (expected {actionCount})");
                isValid = false;
                invalidCount++;
            }
            else
            {
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    if (float.IsNaN(kvp.Value[i]) || float.IsInfinity(kvp.Value[i]))
                    {
                        Debug.LogError($"❌ State '{kvp.Key}' action {i} has invalid value: {kvp.Value[i]}");
                        isValid = false;
                        invalidCount++;
                        break;
                    }
                }
            }
        }

        if (isValid)
        {
            Debug.Log($"✅ Q-Table is valid! ({qTable.Count} entries)");
        }
        else
        {
            Debug.LogError($"❌ Q-Table has {invalidCount} invalid entries!");
        }
    }

    [ContextMenu("Delete Q-Table File")]
    public void DeleteQTableFile()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log($"🗑️ Deleted Q-Table: {SavePath}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to delete: {ex.Message}");
        }
    }

    private int ArgMax(float[] arr)
    {
        int bestIndex = 0;
        float bestValue = arr[0];
        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] > bestValue)
            {
                bestValue = arr[i];
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}