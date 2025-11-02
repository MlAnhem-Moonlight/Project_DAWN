using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SerializableQTable
{
    public List<string> keys = new List<string>();
    public List<float[]> values = new List<float[]>();

    public SerializableQTable(Dictionary<string, float[]> qTable)
    {
        foreach (var kvp in qTable)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<string, float[]> ToDictionary()
    {
        Dictionary<string, float[]> dict = new Dictionary<string, float[]>();
        for (int i = 0; i < keys.Count; i++)
            dict[keys[i]] = values[i];
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

    [Header("Debug")]
    [Tooltip("Bật để xem chi tiết save/load")]
    public bool verboseLogging = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, "qtable.json");
    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();

    private string lastState;
    private int lastAction;
    private bool hasUnsavedChanges = false;

    void OnApplicationQuit()
    {
        if (hasUnsavedChanges)
        {
            Debug.Log("💾 Auto-saving Q-Table before quit...");
            SaveQTable();
        }
    }

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

        return action;
    }

    private int ApplyAdaptiveAdjustment(int baseAction)
    {
        int adjustedAction = baseAction;

        if (consecutiveAllyWins >= 2)
        {
            adjustedAction = Mathf.Max(0, baseAction - 1);
            Debug.Log($"📉 Giảm độ khó: {baseAction} → {adjustedAction} (Ally thắng {consecutiveAllyWins} trận liên tiếp)");
        }
        else if (consecutiveAllyWins >= 1)
        {
            if (Random.value < 0.5f)
                adjustedAction = Mathf.Max(0, baseAction - 1);
        }

        if (consecutiveEnemyWins >= 2)
        {
            adjustedAction = Mathf.Min(actionCount - 1, baseAction + 1);
            Debug.Log($"📈 Tăng độ khó: {baseAction} → {adjustedAction} (Enemy thắng {consecutiveEnemyWins} trận liên tiếp)");
        }
        else if (consecutiveEnemyWins >= 1)
        {
            if (Random.value < 0.5f)
                adjustedAction = Mathf.Min(actionCount - 1, baseAction + 1);
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

        // ⭐ FIX: Lấy reference và update trực tiếp
        float oldValue = qTable[lastState][lastAction];
        float maxNextQ = qTable[nextState].Max();
        float newValue = oldValue + learningRate * (reward + discountFactor * maxNextQ - oldValue);

        // ⭐ CRITICAL: Update trực tiếp vào dictionary
        qTable[lastState][lastAction] = newValue;

        hasUnsavedChanges = true;

        if (verboseLogging)
        {
            Debug.Log($"✅ Q-Update:\n" +
                     $"  State: {lastState}\n" +
                     $"  Action: {lastAction}\n" +
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

            // ⭐ Verify integrity trước khi save
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

            // ⭐ Verify JSON không rỗng
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

            SerializableQTable s = JsonUtility.FromJson<SerializableQTable>(json);

            // Validation
            if (s == null || s.keys == null || s.values == null)
            {
                Debug.LogError("❌ Failed to deserialize. Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            if (s.keys.Count != s.values.Count)
            {
                Debug.LogError($"❌ Corrupted: keys={s.keys.Count}, values={s.values.Count}. Resetting.");
                qTable = new Dictionary<string, float[]>();
                File.Delete(SavePath);
                return;
            }

            // Verify arrays
            for (int i = 0; i < s.values.Count; i++)
            {
                if (s.values[i] == null || s.values[i].Length != actionCount)
                {
                    Debug.LogError($"❌ Entry {i} ({s.keys[i]}) invalid. Resetting.");
                    qTable = new Dictionary<string, float[]>();
                    File.Delete(SavePath);
                    return;
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
                Debug.Log($"📝 Sample: {sample.Key} → [{string.Join(", ", sample.Value.Select(v => v.ToString("F2")))}]");
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
                // Check for NaN or Infinity
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

    [ContextMenu("Force Save Test Entry")]
    public void ForceSaveTestEntry()
    {
        qTable["TestState_Medium"] = new float[] { 1.5f, 2.0f, -0.5f, 0.0f };
        hasUnsavedChanges = true;
        SaveQTable();
        Debug.Log("💾 Saved test entry");

        // Verify bằng cách load lại
        LoadQTable();
        if (qTable.ContainsKey("TestState_Medium"))
        {
            Debug.Log($"✅ Test entry verified: [{string.Join(", ", qTable["TestState_Medium"])}]");
        }
        else
        {
            Debug.LogError("❌ Test entry not found after load!");
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