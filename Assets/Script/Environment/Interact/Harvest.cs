using UnityEngine;

public class Harvest : MonoBehaviour
{
    [Header("Prefab hiển thị khi Player ở gần")]
    public GameObject displayPrefab;

    [Header("Thời gian giữ phím E để thu hoạch (giây)")]
    public float holdSeconds = 2f;

    private GameObject _displayInstance;
    private bool _playerInRange = false;
    private float _holdTimer = 0f;

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        _playerInRange = true;
    //        if (displayPrefab != null && _displayInstance == null)
    //        {
    //            Vector3 spawnPos = other.transform.position + Vector3.up * 1f;
    //            _displayInstance = Instantiate(displayPrefab, spawnPos, Quaternion.identity, transform);
    //            _displayInstance.SetActive(true);
    //        }
    //    }
    //}

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player is holding E to harvest");
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= holdSeconds)
                {
                    Debug.Log("Harvested");
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.Log("Player release E");
                _holdTimer = 0f;
            }
        }
    }

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        _playerInRange = false;
    //        _holdTimer = 0f;
    //        if (_displayInstance != null)
    //        {
    //            Destroy(_displayInstance);
    //            _displayInstance = null;
    //        }
    //    }
    //}
}
