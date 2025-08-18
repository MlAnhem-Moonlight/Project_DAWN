using UnityEngine;

public class MaterialSpawner : MonoBehaviour
{
    [Header("Tên pool chứa các prefab")]
    public string poolName = "Tree"; // Pool này có thể chứa nhiều prefab khác nhau

    [Header("Điểm spawn trên map")]
    public Transform[] spawnPoints;

    [Header("Số lượng spawn mỗi lần")]
    public int totalSpawnCount = 5; // Tổng số object muốn spawn mỗi lần
    [Range(0f, 1f)]
    public float firstPrefabRatio = 0.5f; // Tỷ lệ prefab 1 trong pool (còn lại là prefab 2)

    [Header("Test trong Inspector")]
    public bool spawnNow = false;

    [Header("Parent Object")]
    public GameObject parentObject; // GameObject cha để gom nhóm các object được spawn

    [Header("Daily Refresh")]
    public bool refreshDaily = true;
    private float nextRefreshTime;
    private float dayLength = 24f * 60f; // 24 minutes = 1 game day

    private void Start()
    {
        nextRefreshTime = Time.time + dayLength;
        SpawnMaterials(); // Initial spawn
    }

    private void Update()
    {
        if (spawnNow)
        {
            spawnNow = false; // reset nút
            SpawnMaterials();
        }

        // Check for daily refresh
        if (refreshDaily && Time.time >= nextRefreshTime)
        {
            RefreshAllMaterials();
            nextRefreshTime = Time.time + dayLength;
        }
    }

    public void SpawnMaterials()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("Chưa gán điểm spawn!");
            return;
        }

        if (totalSpawnCount > spawnPoints.Length)
        {
            Debug.LogWarning("Số lượng spawn vượt quá số điểm spawn!");
            totalSpawnCount = spawnPoints.Length; // Giới hạn lại
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

        int prefab1Count = Mathf.RoundToInt(totalSpawnCount * firstPrefabRatio);
        int pointIndex = 0;

        for (int i = 0; i < prefab1Count; i++)
        {
            GameObject spawnedObj = ObjectPooler.Instance.GetFromPool(poolName,  shuffledPoints[pointIndex].position);
            if (spawnedObj != null && parentObject != null)
            {
                spawnedObj.transform.SetParent(parentObject.transform);
            }
            pointIndex++;
        }

    }

    private void RefreshAllMaterials()
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
}
