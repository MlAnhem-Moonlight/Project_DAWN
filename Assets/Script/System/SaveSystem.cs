using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [System.Serializable]
    public class BuildingData
    {
        public string buildingType;      // "Fortress", "Watchtown", etc.
        public bool isBuilt;
        public int constructionHP;
    }

    [System.Serializable]
    public class AllyData
    {
        public string unitType;
        public int level;
        public Vector3 position;
    }

    [System.Serializable]
    public class PlayerData
    {
        public Vector3 position;
        public int level;
        public float currentHP;
        public float currentDMG;
        public float currentSPD;
        public float currentAtkSpd;
        public float currentShield;
        public float currentSkillCD;
        public float currentSkillDmg;
    }

    [System.Serializable]
    public class SaveData
    {
        public int saveIndex;
        public int currentLevel;
        public string mode;
        public PlayerData playerData;
        public List<AllyData> alliesData = new List<AllyData>();
        public List<BuildingData> buildingsData = new List<BuildingData>();
        public IngridientManager.InventoryData inventoryData;
    }

    public List<Ingredient.IngredientEntry> playerResources = new List<Ingredient.IngredientEntry>();
    public List<Ingredient.IngredientEntry> consumedResources = new List<Ingredient.IngredientEntry>();

    /// <summary>
    /// Save toàn bộ dữ liệu: player position, stats, allies, buildings, inventory
    /// </summary>
    public static bool SavePlayerData(
        int saveIndex,
        int currentLevel,
        string mode,
        PlayerStats playerStats,
        List<GameObject> allyList,
        List<BuildConstruction> buildingList,
        List<Ingredient.IngredientEntry> inventory)
    {
        try
        {
            var saveData = new SaveData
            {
                saveIndex = saveIndex,
                currentLevel = currentLevel,
                mode = mode,
                playerData = new PlayerData
                {
                    position = playerStats.transform.position,
                    level = playerStats.level,
                    currentHP = playerStats.currentHP,
                    currentDMG = playerStats.currentDMG,
                    currentSPD = playerStats.currentSPD,
                    currentAtkSpd = playerStats.currentAtkSpd,
                    currentShield = playerStats.currentShield,
                    currentSkillCD = playerStats.currentSkillCD,
                    currentSkillDmg = playerStats.currentSkillDmg
                },
                inventoryData = new IngridientManager.InventoryData()
            };

            // Lưu thông tin allies
            foreach (var ally in allyList)
            {
                if (ally == null || !ally.activeSelf) continue;

                Stats allyStats = ally.GetComponent<Stats>();
                if (allyStats != null)
                {
                    saveData.alliesData.Add(new AllyData
                    {
                        unitType = ally.name.Replace("(Clone)", "").Trim(),
                        level = allyStats.level,
                        position = ally.transform.position
                    });
                }
            }

            // Lưu thông tin buildings
            foreach (var building in buildingList)
            {
                if (building == null) continue;

                saveData.buildingsData.Add(new BuildingData
                {
                    buildingType = building.buildingType.ToString(),
                    isBuilt = building.isBuilt,
                    constructionHP = building.constructionHP
                });
            }

            // Lưu inventory
            foreach (var entry in inventory)
            {
                switch (entry.type.ToLower())
                {
                    case "wood": saveData.inventoryData.wood = entry.quantity; break;
                    case "stone": saveData.inventoryData.stone = entry.quantity; break;
                    case "iron": saveData.inventoryData.iron = entry.quantity; break;
                    case "gold": saveData.inventoryData.gold = entry.quantity; break;
                    case "meat": saveData.inventoryData.meat = entry.quantity; break;
                }
            }

            string json = JsonUtility.ToJson(saveData, true);
            string fileName = $"Save_{saveIndex}.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);

            File.WriteAllText(path, json);
            Debug.Log($"✅ Save successful: {path}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Save failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load dữ liệu save và debug ra
    /// </summary>
    public static SaveData LoadPlayerData(int saveIndex)
    {
        try
        {
            string fileName = $"Save_{saveIndex}.json";
            string path = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"⚠️ Save file not found: {path}");
                return null;
            }

            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogError("❌ Failed to deserialize save data");
                return null;
            }

            // 🟩 DEBUG: In ra những phần đã load được
            Debug.Log($"\n=== 📂 SAVE DATA LOADED (Index: {saveIndex}) ===");
            Debug.Log($"Level: {saveData.currentLevel}");
            Debug.Log($"Mode: {saveData.mode}");

            Debug.Log($"\n👤 PLAYER DATA:");
            Debug.Log($"  Position: {saveData.playerData.position}");
            Debug.Log($"  Level: {saveData.playerData.level}");
            Debug.Log($"  HP: {saveData.playerData.currentHP}");
            Debug.Log($"  DMG: {saveData.playerData.currentDMG}");
            Debug.Log($"  SPD: {saveData.playerData.currentSPD}");
            Debug.Log($"  ATK Speed: {saveData.playerData.currentAtkSpd}");
            Debug.Log($"  Shield: {saveData.playerData.currentShield}");
            Debug.Log($"  Skill CD: {saveData.playerData.currentSkillCD}");
            Debug.Log($"  Skill DMG: {saveData.playerData.currentSkillDmg}");

            Debug.Log($"\n👥 ALLIES ({saveData.alliesData.Count} units):");
            for (int i = 0; i < saveData.alliesData.Count; i++)
            {
                var ally = saveData.alliesData[i];
                Debug.Log($"  [{i}] {ally.unitType} - Level {ally.level} at {ally.position}");
            }

            Debug.Log($"\n🏰 BUILDINGS ({saveData.buildingsData.Count} buildings):");
            for (int i = 0; i < saveData.buildingsData.Count; i++)
            {
                var building = saveData.buildingsData[i];
                Debug.Log($"  [{i}] {building.buildingType} - Built: {building.isBuilt}, HP: {building.constructionHP}");
            }

            Debug.Log($"\n💰 INVENTORY:");
            Debug.Log($"  Wood: {saveData.inventoryData.wood}");
            Debug.Log($"  Stone: {saveData.inventoryData.stone}");
            Debug.Log($"  Iron: {saveData.inventoryData.iron}");
            Debug.Log($"  Gold: {saveData.inventoryData.gold}");
            Debug.Log($"  Meat: {saveData.inventoryData.meat}");
            Debug.Log($"=== End of Save Data ===\n");

            return saveData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Load failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Load default level data
    /// </summary>
    public static SaveData LoadDefaultData()
    {
        Debug.LogWarning("⚠️ LoadDefaultData not yet implemented");
        return null;
    }

    /// <summary>
    /// Wrapper method để gọi từ IngredientManager
    /// </summary>
    public void SaveGameState(
        int currentLevel,
        string mode,
        PlayerStats playerStats,
        List<GameObject> allyList,
        List<BuildConstruction> buildingList,
        List<Ingredient.IngredientEntry> inventory,
        int saveIndex)
    {
        bool success = SavePlayerData(saveIndex, currentLevel, mode, playerStats, allyList, buildingList, inventory);
        if (!success)
        {
            Debug.LogError("Failed to save game state!");
        }
    }

    /// <summary>
    /// Wrapper method để gọi từ IngredientManager
    /// </summary>
    public SaveData LoadGameState(int saveIndex)
    {
        return LoadPlayerData(saveIndex);
    }
}
