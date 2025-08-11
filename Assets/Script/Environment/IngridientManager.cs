using UnityEngine;
using System.Collections.Generic;

public class IngridientManager : MonoBehaviour
{
    [Header("Tài nguyên của người chơi")]
    public List<Ingredient.IngredientEntry> playerIngredients = new List<Ingredient.IngredientEntry>();

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

    // Giảm số lượng tài nguyên
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
}
