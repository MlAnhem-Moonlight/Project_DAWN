using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabConfig
{
    public GameObject prefab;

    [Header("Sorting")]
    public bool autoSortByY = true;
    public string sortingLayerName = "Default";
    public int baseOrderInLayer = 0;
    public Color defaultColor = Color.white;
}

[System.Serializable]
public class PoolItem
{
    public string poolName;
    public PrefabConfig[] prefabs; // Nhiều prefab + config sorting riêng
    public int initialSize = 10;   // Đảm bảo có biến này cho khởi tạo pool
    public bool expandable = true;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("Danh sách pool")]
    public List<PoolItem> itemsToPool;

    private Dictionary<string, List<GameObject>> pooledObjects;

    void Awake()
    {
        if (Instance == null) Instance = this;
        pooledObjects = new Dictionary<string, List<GameObject>>();

        // Khởi tạo tất cả pool
        foreach (var item in itemsToPool)
        {
            List<GameObject> objectList = new List<GameObject>();

            for (int i = 0; i < item.initialSize; i++)
            {
                int prefabIndex = i % item.prefabs.Length;
                GameObject prefab = item.prefabs[prefabIndex].prefab;
                if (prefab == null) continue;
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectList.Add(obj);
            }

            pooledObjects.Add(item.poolName, objectList);
        }
    }

    // Lấy object từ pool với chỉ số prefab cụ thể
    public GameObject GetFromPool(string poolName, int prefabIndex, Vector3 position)
    {
        var itemConfig = itemsToPool.Find(x => x.poolName == poolName);
        if (itemConfig == null || prefabIndex < 0 || prefabIndex >= itemConfig.prefabs.Length)
            return null;

        var list = pooledObjects[poolName];

        foreach (var obj in list)
        {
            if (!obj.activeInHierarchy)
            {
                SetupObject(obj, position, itemConfig.prefabs[prefabIndex]);
                return obj;
            }
        }

        // Nếu không có object trống và được phép mở rộng
        if (itemConfig.expandable)
        {
            GameObject prefab = itemConfig.prefabs[prefabIndex].prefab;
            if (prefab == null) return null;
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            SetupObject(obj, position, itemConfig.prefabs[prefabIndex]);
            list.Add(obj);
            return obj;
        }

        return null;
    }

    private void SetupObject(GameObject obj, Vector3 position, PrefabConfig config)
    {
        obj.transform.position = position;
        obj.SetActive(true);

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = config.sortingLayerName;
            sr.color = config.defaultColor;

            if (config.autoSortByY)
                sr.sortingOrder = config.baseOrderInLayer + Mathf.RoundToInt(-position.y * 100);
            else
                sr.sortingOrder = config.baseOrderInLayer;
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
