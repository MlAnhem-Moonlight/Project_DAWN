using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabConfig
{
    [Header("Sorting")]
    //public bool autoSortByY = true;
    public string sortingLayerName = "Default";
    public int baseOrderInLayer = 0;

    [Header("Color Settings")]
    public Color possibleColors ;

    
}

[System.Serializable]
public class PoolItem
{
    public string poolName;
    public GameObject prefab; // Chỉ cần 1 prefab
    public PrefabConfig[] configPool; // Bể config
    public int initialSize = 10;
    public bool expandable = true;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    public Transform parentObject;

    [Header("Danh sách pool")]
    public List<PoolItem> itemsToPool;

    private Dictionary<string, List<GameObject>> pooledObjects;
    private Dictionary<GameObject, int> objectConfigIndex = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        pooledObjects = new Dictionary<string, List<GameObject>>();

        foreach (var item in itemsToPool)
        {
            List<GameObject> objectList = new List<GameObject>();
            for (int i = 0; i < item.initialSize; i++)
            {
                GameObject obj = Instantiate(item.prefab, parentObject);
                obj.SetActive(false);
                objectList.Add(obj);
            }
            pooledObjects.Add(item.poolName, objectList);
        }
    }

    // Spawn object với config ngẫu nhiên
    public GameObject GetFromPool(string poolName, Vector3 position)
    {
        var item = itemsToPool.Find(x => x.poolName == poolName);
        if (item == null || item.configPool == null || item.configPool.Length == 0)
            return null;

        var list = pooledObjects[poolName];
        int configIndex = Random.Range(0, item.configPool.Length);

        foreach (var obj in list)
        {
            if (!obj.activeInHierarchy)
            {
                SetupObject(obj, position, item.configPool[configIndex]);
                objectConfigIndex[obj] = configIndex;
                return obj;
            }
        }

        if (item.expandable)
        {
            GameObject obj = Instantiate(item.prefab, position, Quaternion.identity);
            SetupObject(obj, position, item.configPool[configIndex]);
            objectConfigIndex[obj] = configIndex;
            list.Add(obj);
            return obj;
        }

        return null;
    }

    // Hàm public để đổi config của object (có thể gọi từ script thời gian)
    public void RefreshObjectConfig(GameObject obj, string poolName)
    {
        var item = itemsToPool.Find(x => x.poolName == poolName);
        if (item == null || item.configPool == null || item.configPool.Length == 0)
            return;

        int configIndex = Random.Range(0, item.configPool.Length);
        SetupObject(obj, obj.transform.position, item.configPool[configIndex]);
        objectConfigIndex[obj] = configIndex;
    }

    private void SetupObject(GameObject obj, Vector3 position, PrefabConfig config)
    {
        obj.transform.position = position;
        obj.SetActive(true);

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = config.sortingLayerName;
            sr.sortingOrder = config.baseOrderInLayer; //config.autoSortByY
                                                       //? config.baseOrderInLayer + Mathf.RoundToInt(-position.y * 100)
                                                       //: config.baseOrderInLayer;

            sr.color = config.possibleColors;

        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    // New helpers to enable/disable all pooled objects (used for performance testing)
    public void SetAllPooledActive(bool active)
    {
        if (pooledObjects == null) return;
        foreach (var kvp in pooledObjects)
        {
            var list = kvp.Value;
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i];
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }
    }

    public int GetTotalPooledCount()
    {
        if (pooledObjects == null) return 0;
        int c = 0;
        foreach (var kvp in pooledObjects) c += kvp.Value?.Count ?? 0;
        return c;
    }
}
