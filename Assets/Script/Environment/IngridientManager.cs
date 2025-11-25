using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using TMPro;

public enum gameMode
{
    easy,
    normal,
    hard
}

public class IngridientManager : MonoBehaviour
{
    [Serializable]
    public class InventoryData
    {
        public int Wood;
        public int Stone;
        public int Iron;
        public int Gold;
        public int Meat;
    }

    [Serializable]
    public class DefaultLevelEntry
    {
        public string Mode;
        public InventoryData Inventory;
    }

    [Serializable]
    public class DefaultLevelRoot
    {
        public DefaultLevelEntry[] DefaultLevel;
    }

    [Serializable]
    public class SaveFileData
    {
        public int Level;
        public string Mode;
        public InventoryData playerResources;
        public InventoryData consumedResources;
    }

    [Header("Tài nguyên của người chơi")]
    public List<Ingredient.IngredientEntry> playerIngredients = new List<Ingredient.IngredientEntry>();

    [Header("Tài nguyên đã tiêu hao")]
    public List<Ingredient.IngredientEntry> consumedResources = new List<Ingredient.IngredientEntry>();

    [Header("Level")]
    public int currentLevel = 0;
    public gameMode mode = gameMode.easy;

    [Header("Save Index")]
    public int saveIndex = 0;

    [Header("Text hiển thị số lượng tài nguyên")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI meatText;
    public TextMeshProUGUI saveFeedbackText; 

    private void Start()
    {
        ResetConsumedResources();
        DisplayResources();
        LoadSave(currentLevel,mode);
    }

    public void LoadSave(int level, gameMode loadMode)
    {
        currentLevel = level;
        mode = loadMode;

        // Level 0 -> load default from Resources/DefaultLevel.json
        if (level == 0)
        {
            // DefaultLevel.json must be placed in Assets/Resources/DefaultLevel.json
            TextAsset txt = Resources.Load<TextAsset>("DefaultLevel");
            if (txt == null)
            {
                Debug.LogError("DefaultLevel.json not found in Resources folder.");
                return;
            }

            try
            {
                var root = JsonUtility.FromJson<DefaultLevelRoot>(txt.text);
                if (root?.DefaultLevel == null || root.DefaultLevel.Length == 0)
                {
                    Debug.LogError("DefaultLevel.json has unexpected format.");
                    return;
                }

                // find entry matching mode string (case-insensitive)
                DefaultLevelEntry selected = null;
                string modeStr = loadMode.ToString().ToLower();
                foreach (var entry in root.DefaultLevel)
                {
                    if (entry.Mode != null && entry.Mode.ToLower() == modeStr)
                    {
                        selected = entry;
                        break;
                    }
                }

                if (selected == null)
                {
                    // fallback: pick first
                    selected = root.DefaultLevel[0];
                }

                playerIngredients = InventoryDataToEntries(selected.Inventory);
                ResetConsumedResources();
                DisplayResources();
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse DefaultLevel.json: " + ex.Message);
            }
        }
        else
        {
            // Load Save_{saveIndex}.json from persistent path
            string fileName = $"Save_{saveIndex}.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Save file not found at {path}. Falling back to default for current mode.");
                // fallback to default if available
                LoadSave(0, loadMode);
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var saveData = JsonUtility.FromJson<SaveFileData>(json);
                if (saveData == null)
                {
                    Debug.LogError("Failed to deserialize save file: " + path);
                    return;
                }

                // Update fields
                currentLevel = saveData.Level;
                // Try parse mode
                if (!string.IsNullOrEmpty(saveData.Mode))
                {
                    if (Enum.TryParse<gameMode>(saveData.Mode, true, out var parsedMode))
                        mode = parsedMode;
                }

                playerIngredients = InventoryDataToEntries(saveData.playerResources);
                consumedResources = InventoryDataToEntries(saveData.consumedResources);

                DisplayResources();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error reading save file: " + ex.Message);
            }
        }
    }


    // Reset tài nguyên tiêu hao về 0
    public void ResetConsumedResources()
    {
        consumedResources.Clear();
        consumedResources.Add(new Ingredient.IngredientEntry { type = "wood", quantity = 0 });
        consumedResources.Add(new Ingredient.IngredientEntry { type = "stone", quantity = 0 });
        consumedResources.Add(new Ingredient.IngredientEntry { type = "iron", quantity = 0 });
        consumedResources.Add(new Ingredient.IngredientEntry { type = "gold", quantity = 0 });
        consumedResources.Add(new Ingredient.IngredientEntry { type = "meat", quantity = 0 });
    }

    public void SetLevel(int level)
    {
        currentLevel = level;
    }

    // Lấy dữ liệu từ save
    public void getDataFromSave()
    {
        if (currentLevel == 0)
        {
            ResetConsumedResources();
            var saveData = SaveSystem.LoadDefaultData();
            playerIngredients = new List<Ingredient.IngredientEntry>(saveData.playerResources);
        }
        else
        {
            var saveData = SaveSystem.LoadPlayerData(saveIndex);
            if (saveData != null)
            {
                playerIngredients = new List<Ingredient.IngredientEntry>(saveData.playerResources);
                consumedResources = new List<Ingredient.IngredientEntry>(saveData.consumedResources);
            }
        }
        DisplayResources();
    }

    // Thêm tài nguyên mới hoặc tăng số lượng
    public void AddIngredient(string typePlus, int amount)
    {
        Debug.Log(typePlus+"aaa");
        for (int i = 0; i < playerIngredients.Count; i++)
        {                
            Debug.Log(playerIngredients[i].type);

            if (playerIngredients[i].type == typePlus)
            {
                var entry = playerIngredients[i];
                entry.quantity += amount;
                playerIngredients[i] = entry;
                DisplayResources();
                return;
            }
        }
        playerIngredients.Add(new Ingredient.IngredientEntry { type = typePlus, quantity = amount });
        DisplayResources();
    }

    // Giảm tài nguyên và cập nhật tiêu hao
    public bool RemoveIngredient(string typePlus, int amount)
    {
        for (int i = 0; i < playerIngredients.Count; i++)
        {
            if (playerIngredients[i].type == typePlus)
            {
                var entry = playerIngredients[i];
                if (entry.quantity >= amount)
                {
                    entry.quantity -= amount;
                    playerIngredients[i] = entry;

                    for (int j = 0; j < consumedResources.Count; j++)
                    {
                        if (consumedResources[j].type.ToLower() == typePlus.ToLower())
                        {
                            var consumedEntry = consumedResources[j];
                            consumedEntry.quantity += amount;
                            consumedResources[j] = consumedEntry;
                            break;
                        }
                    }

                    DisplayResources();
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    private List<Ingredient.IngredientEntry> InventoryDataToEntries(InventoryData inv)
    {
        var list = new List<Ingredient.IngredientEntry>
    {
        new Ingredient.IngredientEntry { type = "wood", quantity = inv != null ? inv.Wood : 0 },
        new Ingredient.IngredientEntry { type = "stone", quantity = inv != null ? inv.Stone : 0 },
        new Ingredient.IngredientEntry { type = "iron", quantity = inv != null ? inv.Iron : 0 },
        new Ingredient.IngredientEntry { type = "gold", quantity = inv != null ? inv.Gold : 0 },
        new Ingredient.IngredientEntry { type = "meat", quantity = inv != null ? inv.Meat : 0 }
    };
        return list;
    }

    private InventoryData EntriesToInventoryData(List<Ingredient.IngredientEntry> entries)
    {
        var inv = new InventoryData();
        foreach (var e in entries)
        {
            if (string.IsNullOrEmpty(e.type)) continue;
            switch (e.type.ToLower())
            {
                case "wood": inv.Wood += e.quantity; break;
                case "stone": inv.Stone += e.quantity; break;
                case "iron": inv.Iron += e.quantity; break;
                case "gold": inv.Gold += e.quantity; break;
                case "meat": inv.Meat += e.quantity; break;
            }
        }
        return inv;
    }

    /// <summary>
    /// Lưu trạng thái hiện tại (Level, Mode, playerResources, consumedResources) ra file Save_{saveIndex}.json
    /// Trả về true nếu lưu thành công.
    /// </summary>
    public bool SavePlayerToDisk()
    {
        try
        {
            var save = new SaveFileData
            {
                Level = currentLevel,
                Mode = mode.ToString(),
                playerResources = EntriesToInventoryData(playerIngredients),
                consumedResources = EntriesToInventoryData(consumedResources)
            };

            string json = JsonUtility.ToJson(save, true); // pretty print = true
            string fileName = $"Save_{saveIndex}.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);

            File.WriteAllText(path, json);
            Debug.Log($"Save successful: {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Save failed: " + ex.Message);
            return false;
        }
    }

    public void SavePlayerToDiskWrapper()
    {
        bool ok = SavePlayerToDisk();
        if (saveFeedbackText != null)
        {
            saveFeedbackText.text = ok ? "Lưu thành công" : "Lưu thất bại";
            saveFeedbackText.gameObject.SetActive(true);
            StopAllCoroutines(); // optional: tránh nhiều coroutine chồng chéo
            StartCoroutine(ClearSaveFeedbackAfterRealtime(2f));
        }
        Debug.Log(ok ? "Save successful (wrapper)" : "Save failed (wrapper)");
    }

    private System.Collections.IEnumerator ClearSaveFeedbackAfterRealtime(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (saveFeedbackText != null)
        {
            saveFeedbackText.text = "";
            saveFeedbackText.gameObject.SetActive(false);
        }
    }



    // Lấy số lượng tài nguyên
    public int GetIngredientAmount(string typePlus)
    {
        foreach (var entry in playerIngredients)
        {
            if (entry.type == typePlus)
                return entry.quantity;
        }
        return 0;
    }

    // Chuyển đổi List<IngredientEntry> sang ResourceData
    public ResourceData ConvertEntriesToResourceData(List<Ingredient.IngredientEntry> entries)
    {
        int wood = 0, stone = 0, iron = 0, gold = 0, meat = 0;
        foreach (var entry in entries)
        {
            switch (entry.type.ToLower())
            {
                case "wood": wood += entry.quantity; break;
                case "stone": stone += entry.quantity; break;
                case "iron": iron += entry.quantity; break;
                case "gold": gold += entry.quantity; break;
                case "meat": meat += entry.quantity; break;
            }
        }
        return new ResourceData(wood, stone, iron, gold, meat);
    }

    // Lấy dữ liệu tài nguyên còn lại
    public ResourceData GetResourceData()
    {
        return ConvertEntriesToResourceData(playerIngredients);
    }

    // Lấy dữ liệu tài nguyên đã tiêu hao
    public ResourceData GetConsumedResourceData()
    {
        return ConvertEntriesToResourceData(consumedResources);
    }

    // Gửi dữ liệu tới ResourceSpawnPredictor
    public void UpdateResourcePredictor()
    {
        var predictor = GetComponent<ResourceSpawnPredictor>();
        if (predictor != null)
        {
            predictor.UpdateTestData(
                GetResourceData(),
                GetConsumedResourceData()
            );
        }
    }

    // 🟩 Hàm hiển thị tài nguyên lên UI
    public void DisplayResources()
    {
        if (woodText != null) woodText.text = GetIngredientAmount("wood").ToString();
        if (stoneText != null) stoneText.text = GetIngredientAmount("stone").ToString();
        if (ironText != null) ironText.text = GetIngredientAmount("iron").ToString();
        if (goldText != null) goldText.text = GetIngredientAmount("gold").ToString();
        if (meatText != null) meatText.text = GetIngredientAmount("meat").ToString();
    }

    // ========================================================
    // KIỂM TRA ĐỦ TÀI NGUYÊN ĐỂ HIRE HERO
    // ========================================================
    public bool CheckEnough(UnitCostLevel cost, int meatCost)
    {
        if (cost == null) return false;

        bool enough =
            GetIngredientAmount("wood") >= cost.wood &&
            GetIngredientAmount("stone") >= cost.stone &&
            GetIngredientAmount("iron") >= cost.iron &&
            GetIngredientAmount("gold") >= cost.gold &&
            GetIngredientAmount("meat") >= meatCost;

        return enough;
    }

}
