using UnityEngine;
using System.Collections.Generic;

public class IngridientManager : MonoBehaviour
{
    [Header("Tài nguyên của người chơi")]
    public List<Ingredient.IngredientEntry> playerIngredients = new List<Ingredient.IngredientEntry>();

    [Header("Tài nguyên đã tiêu hao")]
    public List<Ingredient.IngredientEntry> consumedResources = new List<Ingredient.IngredientEntry>();

    [Header("Level")]
    public int currentLevel = 0;
    [Header("Save Index")]
    public int saveIndex = 0;

    private void Start()
    {
        ResetConsumedResources();
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


    // Lấy dữ liệu từ save, nếu là level 0 thì lấy ở file DefaultLevel.json còn không lấy ở Save.json
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
            var saveData = SaveSystem.LoadPlayerData(saveIndex); // 👉 bạn cần viết hàm này trong SaveSystem
            if (saveData != null)
            {
                playerIngredients = new List<Ingredient.IngredientEntry>(saveData.playerResources);
                consumedResources = new List<Ingredient.IngredientEntry>(saveData.consumedResources);
            }
        }
    }

    // Thêm tài nguyên mới hoặc tăng số lượng
    public void AddIngredient(string typePlus, int amount)
    {
        for (int i = 0; i < playerIngredients.Count; i++)
        {
            if (playerIngredients[i].type == typePlus)
            {
                var entry = playerIngredients[i];
                entry.quantity += amount;
                playerIngredients[i] = entry;
                return;
            }
        }
        playerIngredients.Add(new Ingredient.IngredientEntry { type = typePlus, quantity = amount });
    }

    // Giảm số lượng tài nguyên và cập nhật tài nguyên tiêu hao
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

                    // Cập nhật tài nguyên tiêu hao
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

                    return true;
                }
                return false;
            }
        }
        return false;
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
}
