using UnityEngine;

public class PickingUp : MonoBehaviour
{
    [Header("Assign the prefab to show when player is in range")]
    public GameObject displayPrefab;

    //[Header("Trigger Settings")]
    //public float radius = 2f;

    private GameObject _displayInstance;
    private bool _playerInRange = false;

    //private void Reset()
    //{
    //    // Auto-add a CircleCollider2D set as trigger if not present
    //    CircleCollider2D col = GetComponent<CircleCollider2D>();
    //    if (col == null)
    //        col = gameObject.AddComponent<CircleCollider2D>();
    //    col.isTrigger = true;
    //    col.radius = radius;
    //}

    //private void OnValidate()
    //{
    //    // Update collider radius in editor if changed
    //    CircleCollider2D col = GetComponent<CircleCollider2D>();
    //    if (col != null)
    //    {
    //        col.isTrigger = true;
    //        col.radius = radius;
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("OnTriggerEnter2D called with tag: " + other.tag);
            _playerInRange = true;
            if (displayPrefab != null && _displayInstance == null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1f;
                _displayInstance = Instantiate(displayPrefab, spawnPos, Quaternion.identity);
                _displayInstance.SetActive(true);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (_displayInstance != null)
            {
                Destroy(_displayInstance);
                _displayInstance = null;
            }
        }
    }
}
