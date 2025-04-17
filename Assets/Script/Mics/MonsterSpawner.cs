using UnityEngine;

public class RandomSpawnManager : MonoBehaviour
{
    public GameObject[] enemies; // Array chứa các prefab quái
    public Transform spawnPoint; // Vị trí spawn quái
    public float spawnInterval = 5f; // Thời gian chờ giữa các lần spawn
    public int maxEnemiesPerSpawn = 4; // Số lượng quái tối đa sinh ra mỗi lần
    public int maxEnemiesOnField = 15; // Giới hạn số lượng quái trên sân

    private float spawnTimer = 10f;

    void Update()
    {
        // Tăng thời gian
        spawnTimer += Time.deltaTime;

        // Khi đến thời gian spawn
        if (spawnTimer >= spawnInterval)
        {
            if (CanSpawn())
            {
                SpawnEnemies();
            }
            spawnTimer = 0f; // Đặt lại thời gian
        }
    }

    bool CanSpawn()
    {
        // Lấy tất cả các GameObject hiện tại trên sân
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int demonCount = 0;

        // Kiểm tra các đối tượng có Layer là "Demon"
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Demon"))
            {
                demonCount++;
            }
        }

        // Chỉ spawn nếu số lượng Demon nhỏ hơn maxEnemiesOnField
        return demonCount < maxEnemiesOnField;
    }

    void SpawnEnemies()
    {
        // Random số lượng quái để spawn, tối đa là maxEnemiesPerSpawn
        int enemiesToSpawn = Random.Range(1, maxEnemiesPerSpawn + 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Kiểm tra xem số lượng quái có vượt quá giới hạn sau lượt spawn hay không
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int currentDemonCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == LayerMask.NameToLayer("Demon"))
                {
                    currentDemonCount++;
                }
            }

            // Nếu việc spawn quái tiếp theo vượt quá maxEnemiesOnField, dừng lại
            if (currentDemonCount >= maxEnemiesOnField)
            {
                Debug.Log("Không thể spawn thêm quái! Đã đạt giới hạn trên sân.");
                return;
            }

            // Chọn ngẫu nhiên prefab quái
            GameObject randomEnemy = enemies[Random.Range(0, enemies.Length)];

            // Chọn vị trí spawn
            Transform randomSpawnPoint = spawnPoint;

            // Instantiate (spawn) quái tại vị trí
            GameObject spawnedEnemy = Instantiate(randomEnemy, randomSpawnPoint.position, randomSpawnPoint.rotation);

            // Gán Layer cho quái đã spawn (nếu chưa được đặt)
            spawnedEnemy.layer = LayerMask.NameToLayer("Demon");
        }
    }
}
