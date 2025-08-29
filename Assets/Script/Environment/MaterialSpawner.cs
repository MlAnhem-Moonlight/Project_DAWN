using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Ingredient;

[System.Serializable]
public class GAEntry
{
    public int Tree;
    public int Rock;
    public int Pebble;
    public int Branch;
    public int Bush;
    public int Ore;
    public int Wolf;
    public int Deer;
}

[System.Serializable]
public class GAResultWrapper
{
    public List<GAEntry> GA_Result;
}


public class MaterialSpawner : MonoBehaviour
{
    [Header("JSON Data File")]
    public TextAsset jsonFile; // Kéo thả file JSON vào đây trong Inspector

    [Header("Tên pool chứa các prefab")]
    public string poolName = "Tree";

    [Header("Điểm spawn trên map")]
    public Transform[] spawnPoints;

    [Header("Spawn Configuration")]
    public bool useJsonData = true; // Có sử dụng data từ JSON không
    public int manualSpawnCount = 5; // Số lượng spawn thủ công nếu không dùng JSON

    [Range(0f, 1f)]
    public float firstPrefabRatio = 0.5f; // Giữ lại cho trường hợp không dùng JSON

    [Header("Test trong Inspector")]
    public bool spawnNow = false;

    [Header("Parent Object")]
    public GameObject parentObject;

    // Lưu trữ data từ JSON - Dictionary chứa tất cả pool counts
    public Dictionary<string, int> allPoolCounts = new Dictionary<string, int>();

    private void Start()
    {

    }

    private void Update()
    {
        if (spawnNow)
        {
            spawnNow = false;
            SpawnMaterials();
        }

    }

    private void OnEnable()
    {
        ResourceAllocationGA.onGAResultSaved += OnGAResultSaved;
    }

    private void OnDisable()
    {
        ResourceAllocationGA.onGAResultSaved -= OnGAResultSaved;
    }

    private void OnGAResultSaved()
    {
        LoadJsonData();
        SpawnMaterials();
    }

    private void LoadJsonData()
    {
        if (!useJsonData || jsonFile == null)
        {
            Debug.LogWarning("Không sử dụng JSON data hoặc chưa gán file JSON!");
            return;
        }

        try
        {
            GAResultWrapper wrapper = JsonUtility.FromJson<GAResultWrapper>(jsonFile.text);

            if (wrapper != null && wrapper.GA_Result != null && wrapper.GA_Result.Count > 0)
            {
                allPoolCounts.Clear();

                GAEntry entry = wrapper.GA_Result[0]; // lấy entry đầu tiên
                allPoolCounts["Tree"] = entry.Tree;
                allPoolCounts["Rock"] = entry.Rock;
                allPoolCounts["Pebble"] = entry.Pebble;
                allPoolCounts["Branch"] = entry.Branch;
                allPoolCounts["Bush"] = entry.Bush;
                allPoolCounts["Ore"] = entry.Ore;
                allPoolCounts["Wolf"] = entry.Wolf;
                allPoolCounts["Deer"] = entry.Deer;

                Debug.Log("Đã load GA_Result thành công!");
            }
            else
            {
                Debug.LogError("Không tìm thấy GA_Result hợp lệ trong JSON!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi parse GA_Result: " + e.Message);
        }
        foreach (var kvp in allPoolCounts)
        {
            Debug.Log($"Loaded pool '{kvp.Key}': {kvp.Value}");
        }
    }


    // Trả về spawn count cho pool hiện tại
    private int GetSpawnCountForPool()
    {
        if (useJsonData && allPoolCounts.ContainsKey(poolName))
        {
            return allPoolCounts[poolName];
        }
        else
        {
            if (useJsonData)
            {
                Debug.LogWarning($"Không tìm thấy pool '{poolName}' trong JSON data! Sử dụng manual count: {manualSpawnCount}");
            }
            return manualSpawnCount;
        }
    }

    //// Backup method để parse JSON trực tiếp nếu JsonUtility fail
    //private void TryParseDirectly()
    //{
    //    try
    //    {
    //        string jsonString = jsonFile.text;

    //        // Tìm phần GA_Result trong JSON
    //        int gaResultStart = jsonString.IndexOf("\"GA_Result\"");
    //        if (gaResultStart == -1)
    //        {
    //            Debug.LogError("Không tìm thấy GA_Result trong JSON!");
    //            return;
    //        }

    //        // Tìm phần dictionary đầu tiên trong GA_Result
    //        int firstDictStart = jsonString.IndexOf("{", gaResultStart);
    //        int firstDictEnd = jsonString.IndexOf("}", firstDictStart);

    //        if (firstDictStart == -1 || firstDictEnd == -1)
    //        {
    //            Debug.LogError("Không thể parse GA_Result dictionary!");
    //            return;
    //        }

    //        string dictContent = jsonString.Substring(firstDictStart + 1, firstDictEnd - firstDictStart - 1);

    //        // Parse các key-value pairs
    //        string[] pairs = dictContent.Split(',');
    //        allPoolCounts.Clear();

    //        foreach (string pair in pairs)
    //        {
    //            string[] keyValue = pair.Split(':');
    //            if (keyValue.Length == 2)
    //            {
    //                string key = keyValue[0].Trim().Replace("\"", "");
    //                if (int.TryParse(keyValue[1].Trim(), out int value))
    //                {
    //                    allPoolCounts[key] = value;
    //                    Debug.Log($"Backup parse - Loaded pool '{key}': {value}");
    //                }
    //            }
    //        }

    //        Debug.Log("Parse JSON thành công bằng method backup!");
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError("Lỗi khi parse JSON backup: " + e.Message);
    //    }
    //}

    public void SpawnMaterials()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Chưa gán điểm spawn!");
            return;
        }

        // Xác định số lượng spawn từ JSON data
        int totalSpawnCount = GetSpawnCountForPool();

        if (totalSpawnCount <= 0)
        {
            Debug.LogWarning($"Pool '{poolName}' có spawn count = 0. Không spawn object nào!");
            return;
        }

        if (totalSpawnCount > spawnPoints.Length)
        {
            Debug.LogWarning($"Số lượng spawn ({totalSpawnCount}) vượt quá số điểm spawn ({spawnPoints.Length})! Giới hạn lại.");
            totalSpawnCount = spawnPoints.Length;
        }

        // Tạo bản copy của spawnPoints và shuffle
        Transform[] shuffledPoints = (Transform[])spawnPoints.Clone();
        for (int i = 0; i < shuffledPoints.Length; i++)
        {
            int randIndex = Random.Range(i, shuffledPoints.Length);
            var temp = shuffledPoints[i];
            shuffledPoints[i] = shuffledPoints[randIndex];
            shuffledPoints[randIndex] = temp;
        }

        // Spawn objects
        for (int i = 0; i < totalSpawnCount; i++)
        {
            GameObject spawnedObj = ObjectPooler.Instance.GetFromPool(poolName, shuffledPoints[i].position);
            if (spawnedObj != null && parentObject != null)
            {
                spawnedObj.transform.SetParent(parentObject.transform);
            }
        }

        Debug.Log($"Đã spawn {totalSpawnCount} {poolName} objects!");
    }

    [ContextMenu("Refresh All Materials")]
    public void RefreshAllMaterials()
    {
        // Return all active objects to pool
        if (parentObject != null)
        {
            for (int i = parentObject.transform.childCount - 1; i >= 0; i--)
            {
                var child = parentObject.transform.GetChild(i);
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
            }
        }

        // Respawn all materials
        SpawnMaterials();
    }

    [ContextMenu("Reload JSON Data")]
    public void ReloadJsonData()
    {
        LoadJsonData();
    }

    [ContextMenu("Show Current Pool Count")]
    public void ShowCurrentPoolCount()
    {
        int count = GetSpawnCountForPool();
        Debug.Log($"Pool '{poolName}' sẽ spawn {count} objects");

        if (useJsonData && allPoolCounts.Count > 0)
        {
            Debug.Log("=== All Pool Counts từ GA_Result ===");
            foreach (var kvp in allPoolCounts)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
    }

    // Method mới để get spawn count cho bất kỳ pool nào
    public int GetSpawnCountForSpecificPool(string specificPoolName)
    {
        if (useJsonData && allPoolCounts.ContainsKey(specificPoolName))
        {
            return allPoolCounts[specificPoolName];
        }
        return 0;
    }

    // Method mới để get tất cả pool data
    public Dictionary<string, int> GetAllPoolCounts()
    {
        return new Dictionary<string, int>(allPoolCounts);
    }
}