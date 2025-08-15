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

    private void Update()
    {
        if (spawnNow)
        {
            spawnNow = false; // reset nút
            SpawnMaterials();
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
        int prefab2Count = totalSpawnCount - prefab1Count;

        int pointIndex = 0;

        // Spawn prefab thứ nhất (index = 0)
        for (int i = 0; i < prefab1Count; i++)
        {
            ObjectPooler.Instance.GetFromPool(poolName, 0, shuffledPoints[pointIndex].position);
            pointIndex++;
        }

        // Spawn prefab thứ hai (index = 1)
        for (int i = 0; i < prefab2Count; i++)
        {
            ObjectPooler.Instance.GetFromPool(poolName, 1, shuffledPoints[pointIndex].position);
            pointIndex++;
        }
    }

}
