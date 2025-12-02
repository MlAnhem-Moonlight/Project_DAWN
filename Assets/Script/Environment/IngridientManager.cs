using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using TMPro;
using UnityEngine.EventSystems;

public enum gameMode
{
    easy,
    normal,
    hard
}

public class IngridientManager : MonoBehaviour
{
    [System.Serializable]
    public class InventoryData
    {
        public int wood;
        public int stone;
        public int iron;
        public int gold;
        public int meat;
    }

    [System.Serializable]
    public class DefaultLevelEntry
    {
        public string Mode;
        public InventoryData Inventory;
    }

    [System.Serializable]
    public class DefaultLevelRoot
    {
        public DefaultLevelEntry[] DefaultLevel;
    }

    [Header("Tài nguyên c?a ngu?i choi")]
    public List<Ingredient.IngredientEntry> playerIngredients = new List<Ingredient.IngredientEntry>();

    [Header("Tài nguyên dã tiêu hao")]
    public List<Ingredient.IngredientEntry> consumedResources = new List<Ingredient.IngredientEntry>();

    [Header("Level")]
    public int currentLevel = 0;
    public gameMode mode = gameMode.easy;

    [Header("Save Index")]
    public int saveIndex = 0;

    [Header("References")]
    public PlayerStats playerStats;
    public GameObject allyManager;
    public GameObject buildingContainer;  // Parent ch?a t?t c? buildings

    [Header("Text hi?n th? s? lu?ng tài nguyên")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI meatText;
    public TextMeshProUGUI saveFeedbackText;

    private void Start()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();

        if (allyManager == null)
            allyManager = GameObject.Find("AllyUnitController");

        ResetConsumedResources();
    }

    public void LoadSave()
    {

        // Load t? SaveSystem m?i
        GameObject btn = EventSystem.current.currentSelectedGameObject;
        saveIndex = btn.name switch
        {
            "Save_1" => 1,
            "Save_2" => 2,
            "Save_3" => 3,
            "Save_4" => 4,
            "Save_5" => 5,
            "Save_6" => 6,
            _ => 0,
        };
        var loadedData = SaveSystem.LoadPlayerData(saveIndex);
        if (loadedData == null)
        {
            Debug.LogWarning($"Save file not found. ");
            return;
        }

        // C?p nh?t inventory
        playerIngredients = InventoryDataToEntries(loadedData.inventoryData);
        ResetConsumedResources();
        DisplayResources();

        // C?p nh?t player position và stats
        if (playerStats != null)
        {
            playerStats.transform.position = loadedData.playerData.position;
            playerStats.level = loadedData.playerData.level;
            playerStats.currentHP = loadedData.playerData.currentHP;
            playerStats.currentDMG = loadedData.playerData.currentDMG;
            playerStats.currentSPD = loadedData.playerData.currentSPD;
            playerStats.currentAtkSpd = loadedData.playerData.currentAtkSpd;
            playerStats.currentShield = loadedData.playerData.currentShield;
            playerStats.currentSkillCD = loadedData.playerData.currentSkillCD;
            playerStats.currentSkillDmg = loadedData.playerData.currentSkillDmg;
            playerStats.updateHP();
            Debug.Log("? Player data loaded and applied");
        }

        // Load buildings
        ApplyBuildingsData(loadedData.buildingsData);

        Debug.Log($"?? Loaded {loadedData.alliesData.Count} allies - Spawn logic TODO");

        DisplayResources();

    }

    public void LoadDefaultInventory(gameMode loadMode)
    {
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

    /// <summary>
    /// Apply buildings data t? save
    /// </summary>
    private void ApplyBuildingsData(List<SaveSystem.BuildingData> buildingsData)
    {
        if (buildingContainer == null)
        {
            Debug.LogWarning("?? buildingContainer not assigned! Cannot apply building data.");
            return;
        }

        BuildConstruction[] allBuildings = buildingContainer.GetComponentsInChildren<BuildConstruction>();
        
        foreach (var buildingData in buildingsData)
        {
            // Tìm building có cùng lo?i
            foreach (var building in allBuildings)
            {
                if (building.buildingType.ToString() == buildingData.buildingType)
                {
                    building.isBuilt = buildingData.isBuilt;
                    building.constructionHP = buildingData.constructionHP;
                    
                    // Áp d?ng tr?ng thái visual
                    if (buildingData.isBuilt)
                    {
                        if (building.construction != null)
                            building.construction.SetActive(true);
                        building.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        if (building.construction != null)
                            building.construction.SetActive(false);
                        building.GetComponent<SpriteRenderer>().enabled = true;
                    }

                    Debug.Log($"? Loaded building {buildingData.buildingType} - Built: {buildingData.isBuilt}, HP: {buildingData.constructionHP}");
                    break;
                }
            }
        }
    }

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

    public void AddIngredient(string typePlus, int amount)
    {
        for (int i = 0; i < playerIngredients.Count; i++)
        {
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
            new Ingredient.IngredientEntry { type = "wood", quantity = inv != null ? inv.wood : 0 },
            new Ingredient.IngredientEntry { type = "stone", quantity = inv != null ? inv.stone : 0 },
            new Ingredient.IngredientEntry { type = "iron", quantity = inv != null ? inv.iron : 0 },
            new Ingredient.IngredientEntry { type = "gold", quantity = inv != null ? inv.gold : 0 },
            new Ingredient.IngredientEntry { type = "meat", quantity = inv != null ? inv.meat : 0 }
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
                case "wood": inv.wood += e.quantity; break;
                case "stone": inv.stone += e.quantity; break;
                case "iron": inv.iron += e.quantity; break;
                case "gold": inv.gold += e.quantity; break;
                case "meat": inv.meat += e.quantity; break;
            }
        }
        return inv;
    }

    /// <summary>
    /// Save toàn b? game state kèm buildings
    /// </summary>
    public bool SavePlayerToDisk()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not assigned!");
            return false;
        }

        // L?y list allies t? allyManager
        List<GameObject> directChildren = new List<GameObject>();
        for (int i = 0; i < allyManager.transform.childCount; i++)
        {
            directChildren.Add(allyManager.transform.GetChild(i).gameObject);
        }

        // L?y list buildings
        List<BuildConstruction> buildingList = new List<BuildConstruction>();
        if (buildingContainer != null)
        {
            BuildConstruction[] allBuildings = buildingContainer.GetComponentsInChildren<BuildConstruction>();
            buildingList.AddRange(allBuildings);
        }
        else
        {
            // Fallback: tìm t?t c? BuildConstruction trong scene
            BuildConstruction[] allBuildings = FindObjectsByType<BuildConstruction>(FindObjectsSortMode.None);
            buildingList.AddRange(allBuildings);
            Debug.LogWarning("?? buildingContainer not assigned! Using FindObjectsOfType instead.");
        }

        Debug.Log($"?? Saving {buildingList.Count} buildings...");

        return SaveSystem.SavePlayerData(
            saveIndex,
            currentLevel,
            mode.ToString(),
            playerStats,
            directChildren,
            buildingList,
            playerIngredients
        );
    }

    public void SavePlayerToDiskWrapper()
    {
        GameObject btn = EventSystem.current.currentSelectedGameObject;
        saveIndex = btn.name switch
        {
            "Save_1" => 1,
            "Save_2" => 2,
            "Save_3" => 3,
            "Save_4" => 4,
            "Save_5" => 5,
            "Save_6" => 6,
            _ => 0,
        };
        bool ok = SavePlayerToDisk();
        if (saveFeedbackText != null)
        {
            saveFeedbackText.text = ok ? "? Luu thành công" : "? Luu th?t b?i";
            saveFeedbackText.gameObject.SetActive(true);
            StopAllCoroutines();
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

    public int GetIngredientAmount(string typePlus)
    {
        foreach (var entry in playerIngredients)
        {
            if (entry.type == typePlus)
                return entry.quantity;
        }
        return 0;
    }

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

    public ResourceData GetResourceData()
    {
        return ConvertEntriesToResourceData(playerIngredients);
    }

    public ResourceData GetConsumedResourceData()
    {
        return ConvertEntriesToResourceData(consumedResources);
    }

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
    [ContextMenu("Display Resources")]
    public void DisplayResources()
    {
        if (woodText != null) woodText.text = GetIngredientAmount("wood").ToString();
        if (stoneText != null) stoneText.text = GetIngredientAmount("stone").ToString();
        if (ironText != null) ironText.text = GetIngredientAmount("iron").ToString();
        if (goldText != null) goldText.text = GetIngredientAmount("gold").ToString();
        if (meatText != null) meatText.text = GetIngredientAmount("meat").ToString();
    }

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
