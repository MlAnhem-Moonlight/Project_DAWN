using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

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

    void Start()
    {
        LoadFromJSON();
    }

    void LoadFromJSON()
    {
        if (environmentData == null)
        {
            LoadEnvironmentData();
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

    public static void LoadEnvironmentData()
    {
        string filePath = Path.Combine(Application.dataPath, "Script/Environment/env.json");
        if (File.Exists(filePath))
        {
            try
            {
                string jsonText = File.ReadAllText(filePath);

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

                        Debug.Log($"Đã load thành công {data.objects.Length} objects từ env.json");
                    }
                    else
                    {
                        Debug.LogError("Không thể tìm thấy mảng 'objects' hợp lệ trong env.json");
                    }
                }
                else
                {
                    Debug.LogError("Không tìm thấy key 'objects' trong env.json");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi khi đọc env.json: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Không tìm thấy file env.json tại: {filePath}");
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

    // Method để save GA results vào file
    public static void SaveGAResult(Dictionary<string, int> result)
    {
        string filePath = Path.Combine(Application.dataPath, "Script/Environment/env.json");

        try
        {
            string jsonText = File.ReadAllText(filePath);

            // Tìm vị trí của "GA_Result"
            int gaResultStart = jsonText.IndexOf("\"GA_Result\":");
            if (gaResultStart >= 0)
            {
                // Tìm vị trí bắt đầu của mảng GA_Result
                int arrayStart = jsonText.IndexOf('[', gaResultStart);
                int arrayEnd = FindMatchingBracket(jsonText, arrayStart);

                if (arrayStart >= 0 && arrayEnd >= 0)
                {
                    // Tạo string mới cho GA_Result
                    string newGAResult = "[\n    {\n";
                    bool first = true;
                    foreach (var kvp in result)
                    {
                        if (!first) newGAResult += ",\n";
                        newGAResult += $"      \"{kvp.Key}\": {kvp.Value}";
                        first = false;
                    }
                    newGAResult += "\n    }\n  ]";

                    // Thay thế phần GA_Result cũ
                    string newJsonText = jsonText.Substring(0, arrayStart) + newGAResult + jsonText.Substring(arrayEnd + 1);

                    File.WriteAllText(filePath, newJsonText);
                    Debug.Log("Đã lưu kết quả GA vào env.json");
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy 'GA_Result' trong file json");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lưu GA result: {e.Message}");
        }
    }
}
