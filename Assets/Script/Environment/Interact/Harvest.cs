using UnityEngine;
using UnityEngine.UI;

public class HarvestInteraction : MonoBehaviour
{
    [Header("Harvest Settings")]
    public float holdSeconds = 3f;

    [Header("UI References")]
    public GameObject uiPrefab; // Prefab chứa UI (Canvas với Image)
    public Image fillCircle; // Reference đến Image component trong prefab

    private float _holdTimer = 0f;
    private bool _playerInRange = false;
    private GameObject _currentUIInstance;
    private Transform _playerTransform;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            _playerTransform = other.transform;
            ShowUI();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            _playerTransform = null;
            HideUI();
            ResetProgress();
        }
    }

    private void Update()
    {
        if (_playerInRange && _playerTransform != null)
        {
            // Cập nhật vị trí UI theo vị trí player
            //UpdateUIPosition();

            if (Input.GetKey(KeyCode.E))
            {
                _holdTimer += Time.deltaTime;
                UpdateFillAmount();

                if (_holdTimer >= holdSeconds)
                {
                    CompleteHarvest();
                }
            }
            else if (_holdTimer > 0)
            {
                // Người chơi thả phím E, reset progress
                ResetProgress();
            }
        }
    }

    private void ShowUI()
    {
        if (uiPrefab != null && _currentUIInstance == null)
        {
            _currentUIInstance = Instantiate(uiPrefab);

            // Nếu chưa có reference đến fillCircle, tìm trong prefab
            if (fillCircle == null && _currentUIInstance != null)
            {
                fillCircle = _currentUIInstance.GetComponentInChildren<Image>();
            }

            //UpdateUIPosition();
        }
    }

    private void HideUI()
    {
        if (_currentUIInstance != null)
        {
            Destroy(_currentUIInstance);
            _currentUIInstance = null;
        }
    }

    //private void UpdateUIPosition()
    //{
    //    if (_currentUIInstance != null && _playerTransform != null)
    //    {
    //        // Tính toán hướng từ object này đến player
    //        Vector3 directionToPlayer = (_playerTransform.position - transform.position).normalized;

    //        // Đặt UI ở vị trí giữa object và player, hoặc offset theo hướng player
    //        Vector3 uiPosition = transform.position + directionToPlayer * 1.5f; // 1.5f là khoảng cách offset

    //        // Chuyển đổi world position sang screen position cho UI
    //        Vector3 screenPosition = Camera.main.WorldToScreenPoint(uiPosition);
    //        _currentUIInstance.transform.position = screenPosition;
    //    }
    //}

    private void UpdateFillAmount()
    {
        if (fillCircle != null)
        {
            fillCircle.fillAmount = _holdTimer / holdSeconds;
        }
    }

    private void ResetProgress()
    {
        _holdTimer = 0f;
        UpdateFillAmount();
    }

    private void CompleteHarvest()
    {
        Debug.Log("Harvested successfully!");
        HideUI();

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

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Đảm bảo UI được ẩn khi object bị disable
        HideUI();
    }
}