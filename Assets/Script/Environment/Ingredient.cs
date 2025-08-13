using UnityEngine;
using System;

public class Ingredient : MonoBehaviour
{
    public string objectId; // set trong Inspector



    [Serializable]
    public struct IngredientEntry
    {
        public string type;
        public int quantity;
    }

    [Serializable]
    public class IngredientObject
    {
        public string id;
        public IngredientEntry[] ingredients;
    }

    [Serializable]
    public class IngredientList
    {
        public IngredientObject[] objects;
    }

    public IngredientEntry[] ingredients;

    void Start()
    {
        LoadFromJSON();
    }

    void LoadFromJSON()
    {
        string filePath = System.IO.Path.Combine(Application.dataPath, "Script/Environment/env.json");
        if (System.IO.File.Exists(filePath))
        {
            string jsonText = System.IO.File.ReadAllText(filePath);
            IngredientList data = JsonUtility.FromJson<IngredientList>(jsonText);

            foreach (var obj in data.objects)
            {
                if (obj.id == objectId)
                {
                    ingredients = obj.ingredients;
                    return;
                }
            }

            Debug.LogWarning($"Không tìm thấy ID '{objectId}' trong env.json");
        }
        else
        {
            Debug.LogError("Không tìm thấy file env.json ở cùng folder với script");
        }
    }
}
