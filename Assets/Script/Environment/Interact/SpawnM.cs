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
    //private IEnumerator WaitForGAResultAndSpawn()
    //{
    //    string gaResultPath = System.IO.Path.Combine(Application.dataPath, "Script/Environment/env.json");
    //    float timeout = 5f;
    //    float timer = 0f;

    //    while (!System.IO.File.Exists(gaResultPath) && timer < timeout)
    //    {
    //        yield return new WaitForSeconds(0.2f);
    //        timer += 0.2f;
    //    }

    //    yield return new WaitForSeconds(0.2f);

    //    LoadJsonData();
    //    SpawnObjects();
    //}

    [ContextMenu("Spawn Now")]
    public void SpawnObjects()
    {
        spawnCount = GetSpawnCountForSpecificPool(poolName);
        if (objectPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(areaStart.transform.position.x, areaEnd.transform.position.x);
            //float y = Random.Range(areaStart.y, areaEnd.y);
            Vector2 spawnPos = new Vector2(x, yOffset);

            Instantiate(objectPrefab, spawnPos, objectPrefab.transform.rotation, PObject.transform);
        }
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

    // Update is called once per frame
    void Update()
    {

    }
}
