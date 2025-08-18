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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnObjects();
    }
    [ContextMenu("Spawn Now")]
    public void SpawnObjects()
    {
        if (objectPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float x = Random.Range(areaStart.transform.position.x, areaEnd.transform.position.x);
            //float y = Random.Range(areaStart.y, areaEnd.y);
            Vector2 spawnPos = new Vector2(x, yOffset);

            Instantiate(objectPrefab, spawnPos, objectPrefab.transform.rotation, PObject.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
