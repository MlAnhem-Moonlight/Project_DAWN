using UnityEngine;

public class PickingUp : MonoBehaviour
{
    [Header("Assign the prefab to show when player is in range")]
    public GameObject displayPrefab;

    [Header("Display Settings")]
    public Vector3 displayOffset = Vector3.up * 1f; // Offset từ vị trí object

    private GameObject _displayInstance;
    private bool _playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered interaction range");
            _playerInRange = true;
            ShowDisplay();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left interaction range");
            _playerInRange = false;
            HideDisplay();
        }
    }

    private void Update()
    {
        // Chỉ kiểm tra input khi player đang trong tầm
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player pressed E - Object picked up");
            PickUpObject();
        }
    }

    private void ShowDisplay()
    {
        if (displayPrefab != null && _displayInstance == null)
        {
            // Tạo display tại vị trí cố định (vị trí object + offset)
            Vector3 displayPosition = transform.position + displayOffset;
            _displayInstance = Instantiate(displayPrefab, displayPosition, Quaternion.identity);
            _displayInstance.SetActive(true);
            Debug.Log("Display shown at fixed position");
        }
    }

    private void HideDisplay()
    {
        if (_displayInstance != null)
        {
            Destroy(_displayInstance);
            _displayInstance = null;
            Debug.Log("Display hidden");
        }
    }

    private void PickUpObject()
    {
        // Ẩn display trước khi tắt object
        HideDisplay();

        // Lấy các nguyên liệu từ Ingredient và đẩy vào IngridientManager
        Ingredient ingredient = GetComponent<Ingredient>();
        IngridientManager manager = Object.FindFirstObjectByType<IngridientManager>();
        if (ingredient != null && manager != null && ingredient.ingredients != null)
        {
            foreach (var entry in ingredient.ingredients)
            {
                manager.AddIngredient(entry.type, entry.quantity);
            }
        }
        // Tắt object
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Đảm bảo display được ẩn khi object bị disable
        HideDisplay();
    }
}