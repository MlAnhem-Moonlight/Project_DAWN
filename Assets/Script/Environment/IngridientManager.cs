using UnityEngine;
using System.Collections.Generic;
using TMPro;

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

    [Header("Text hiển thị số lượng tài nguyên")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI meatText;

    private void Start()
    {
        ResetConsumedResources();
        DisplayResources();
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
}
