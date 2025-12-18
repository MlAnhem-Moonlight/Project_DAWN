using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public string objectId; // set trong Inspector
    public float harvestTime;

    [SerializeField]
    private TextAsset environmentJsonAsset; // Gán file env.json ở Inspector

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

    [Serializable]
    public class GAResultEntry
    {
        public string objectName;
        public int quantity;
    }

    [Serializable]
    public class EnvironmentData
    {
        public IngredientObject[] objects;
        public Dictionary<string, int>[] GA_Result;
    }

    public IngredientEntry[] ingredients;
    private static EnvironmentData environmentData;
    private static TextAsset cachedAsset;



    void Start()
    {
        environmentJsonAsset = environmentJsonAsset ?? Resources.Load<TextAsset>("env");
        LoadFromJSON();
    }

    void LoadFromJSON()
    {
        if (environmentData == null)
        {
            LoadEnvironmentData(environmentJsonAsset);
        }

        if (environmentData != null && environmentData.objects != null)
        {
            foreach (var obj in environmentData.objects)
            {
                if (obj.id == objectId)
                {
                    ingredients = obj.ingredients;
                    return;
                }
            }
            Debug.LogWarning($"Không tìm thấy ID '{objectId}' trong env.json");
        }
    }

    public static void LoadEnvironmentData(TextAsset jsonAsset = null)
    {
        if (jsonAsset == null)
        {
            if (cachedAsset == null)
            {
                cachedAsset = Resources.Load<TextAsset>("env");
            }
            jsonAsset = cachedAsset;
        }
        else
        {
            cachedAsset = jsonAsset;
        }

        if (jsonAsset == null)
        {
            Debug.LogError("❌ Không tìm thấy TextAsset 'env.json' trong Resources folder!");
            return;
        }

        try
        {
            string jsonText = jsonAsset.text;

            // Parse chỉ phần "objects"
            int objectsStart = jsonText.IndexOf("\"objects\":");
            if (objectsStart >= 0)
            {
                int arrayStart = jsonText.IndexOf('[', objectsStart);
                int arrayEnd = FindMatchingBracket(jsonText, arrayStart);

                if (arrayStart >= 0 && arrayEnd >= 0)
                {
                    string objectsArray = jsonText.Substring(arrayStart, arrayEnd - arrayStart + 1);
                    string wrappedJson = "{\"objects\":" + objectsArray + "}";

                    IngredientList data = JsonUtility.FromJson<IngredientList>(wrappedJson);
                    environmentData = new EnvironmentData();
                    environmentData.objects = data.objects;

                    Debug.Log($"✅ Đã load thành công {data.objects.Length} objects từ env.json (TextAsset)");
                }
                else
                {
                    Debug.LogError("❌ Không thể tìm thấy mảng 'objects' hợp lệ trong env.json");
                }
            }
            else
            {
                Debug.LogError("❌ Không tìm thấy key 'objects' trong env.json");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Lỗi khi đọc env.json: {e.Message}");
        }
    }

    // Helper method để tìm dấu ] đóng của mảng objects
    private static int FindMatchingBracket(string json, int openBracketIndex)
    {
        int bracketCount = 0;
        bool inString = false;
        bool isEscaped = false;

        for (int i = openBracketIndex; i < json.Length; i++)
        {
            char c = json[i];

            if (isEscaped)
            {
                isEscaped = false;
                continue;
            }

            if (c == '\\')
            {
                isEscaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString) continue;

            if (c == '[')
                bracketCount++;
            else if (c == ']')
            {
                bracketCount--;
                if (bracketCount == 0)
                    return i;
            }
        }

        return -1;
    }

    // Public method để get environment data
    public static EnvironmentData GetEnvironmentData()
    {
        if (environmentData == null)
        {
            LoadEnvironmentData();
        }
        return environmentData;
    }

    // Add event for GA result saved notification
    public static event System.Action onGAResultSaved;

    // Ví dụ ghi vào file riêng biệt
    public static void SaveGAResult(Dictionary<string, int> result)
    {
        string path = Path.Combine(Application.persistentDataPath, "GA_Result.json");
        string json = JsonUtility.ToJson(new GAResultWrapper { data = result }, true);
        File.WriteAllText(path, json);
        onGAResultSaved?.Invoke();
    }

    [Serializable]
    private class GAResultWrapper
    {
        public Dictionary<string, int> data;
    }
}
