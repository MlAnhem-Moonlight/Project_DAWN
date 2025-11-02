using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class SpawnM : MonoBehaviour
{
    [Header("PoolName")]
    public string poolName = "SpawnM"; // Unique ID for this spawner

    [Header("Spawn Settings")]
    public GameObject objectPrefab, PObject; // Prefab to spawn
    public int spawnCount = 10; // Number of objects to spawn
    public GameObject areaStart, areaEnd;
    public float yOffset = 0f; // Offset for the y position

    [Header("Clear Settings")]
    public bool destroyOldObjects = false; // true = Destroy, false = SetActive(false)

    // Lưu trữ data từ JSON - Dictionary chứa tất cả pool counts
    private Dictionary<string, int> allPoolCounts = new Dictionary<string, int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //StartCoroutine(WaitForGAResultAndSpawn());
    }

    private void OnEnable()
    {
        Ingredient.onGAResultSaved += OnGAResultSaved;
    }

    private void OnDisable()
    {
        Ingredient.onGAResultSaved -= OnGAResultSaved;
    }

    private void OnGAResultSaved()
    {
        LoadJsonData();
        SpawnObjects();
    }

    // Xóa hoặc ẩn toàn bộ object cũ trong PObject
    private void ClearOldObjects()
    {
        if (PObject == null) return;

        int clearedCount = 0;

        if (destroyOldObjects)
        {
            // Destroy tất cả children
            for (int i = PObject.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(PObject.transform.GetChild(i).gameObject);
                clearedCount++;
            }
        }
        else
        {
            // Chỉ ẩn (SetActive false)
            for (int i = PObject.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = PObject.transform.GetChild(i).gameObject;
                if (child.activeSelf)
                {
                    child.SetActive(false);
                    clearedCount++;
                }
            }
        }

        //Debug.Log($"Đã clear {clearedCount} objects cũ từ '{poolName}' (Destroy: {destroyOldObjects})");
    }

    [ContextMenu("Spawn Now")]
    public void SpawnObjects()
    {
        // Clear toàn bộ object cũ trước
        ClearOldObjects();

        spawnCount = GetSpawnCountForSpecificPool(poolName);
        if (objectPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(areaStart.transform.position.x, areaEnd.transform.position.x);
            //float y = Random.Range(areaStart.y, areaEnd.y);
            Vector2 spawnPos = new Vector2(x, yOffset);

            Instantiate(objectPrefab, spawnPos, objectPrefab.transform.rotation, PObject.transform);
        }

        //Debug.Log($"Đã spawn {spawnCount} objects mới cho '{poolName}'");
    }

    private void LoadJsonData()
    {
        string filePath = Path.Combine(Application.dataPath, "Script/Environment/env.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Không tìm thấy file JSON!");
            return;
        }

        try
        {
            string jsonText = File.ReadAllText(filePath);
            GAResultWrapper wrapper = JsonUtility.FromJson<GAResultWrapper>(jsonText);

            if (wrapper != null && wrapper.GA_Result != null && wrapper.GA_Result.Count > 0)
            {
                allPoolCounts.Clear();

                GAEntry entry = wrapper.GA_Result[0];
                allPoolCounts["Tree"] = entry.Tree;
                allPoolCounts["Rock"] = entry.Rock;
                allPoolCounts["Pebble"] = entry.Pebble;
                allPoolCounts["Branch"] = entry.Branch;
                allPoolCounts["Bush"] = entry.Bush;
                allPoolCounts["Ore"] = entry.Ore;
                allPoolCounts["Wolf"] = entry.Wolf;
                allPoolCounts["Deer"] = entry.Deer;

                //Debug.Log("Đã load GA_Result thành công!");
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
    }

    // Method mới để get spawn count cho bất kỳ pool nào
    public int GetSpawnCountForSpecificPool(string specificPoolName)
    {
        if (allPoolCounts.ContainsKey(specificPoolName))
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

    // Context menu để test clear
    [ContextMenu("Clear All Objects")]
    public void ClearAllObjects()
    {
        ClearOldObjects();
    }

    // Context menu để đếm số objects hiện có
    [ContextMenu("Count Current Objects")]
    public void CountCurrentObjects()
    {
        if (PObject == null)
        {
            Debug.Log("PObject chưa được gán!");
            return;
        }

        int totalCount = PObject.transform.childCount;
        int activeCount = 0;

        for (int i = 0; i < PObject.transform.childCount; i++)
        {
            if (PObject.transform.GetChild(i).gameObject.activeSelf)
                activeCount++;
        }

        Debug.Log($"Pool '{poolName}': {activeCount}/{totalCount} objects đang active");
    }

    // Update is called once per frame
    void Update()
    {

    }
}