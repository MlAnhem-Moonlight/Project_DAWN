using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public List<Ingredient.IngredientEntry> playerResources = new List<Ingredient.IngredientEntry>();
    public List<Ingredient.IngredientEntry> consumedResources = new List<Ingredient.IngredientEntry>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //phần này chưa cần dùng đến, sẽ save data vào file Save_X.json (với x là thứ tự file save)
    public static void SavePlayerData(int index)
    {
        // Implement saving player data logic here
        Debug.Log($"Saving player data for level {index}");
    }
    // Phần này sẽ load data từ file Save_X.json (với x là thứ tự file save) và đẩy dữ liệu vào trong SaveSystem
    public static SaveSystem LoadPlayerData(int index)
    {
        string path = Path.Combine(Application.persistentDataPath, $"Save_{index}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveSystem>(json);
        }
        return null;
    }
    //phần này sẽ load data từ file DefaultLevel.json và đẩy dữ liệu vào trong SaveSystem
    public static SaveSystem LoadDefaultData()
    {
        string path = Path.Combine(Application.persistentDataPath, $"DefaultLevel.json");
        // Implement loading player data logic here
        Debug.Log($"Loading player data for level");
        return null; // Replace with actual loaded data
    }
}
