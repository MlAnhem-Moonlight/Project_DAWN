using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI ingredientText;
    public Image progressImage;

    [Header("Follow Player")]
    public Transform player;
    public Vector3 offset = new Vector3(-0.5f, 1f, 0f);

    [Header("References")]
    public IngridientManager ingredientManager; // link tới manager trong scene

    private GameObject currentObject;
    private Ingredient currentIngredient;

    private bool isHolding = false;
    private float holdTimer = 0f;

    void Start()
    {
        Clear();
    }

    void Update()
    {
        HandleHarvestInput();
        FollowTarget();
        FollowPlayer();
    }

    void FollowTarget()
    {
        Vector3 offset2 = new Vector3(0f, 2f, 0f);
        if (progressImage != null && currentObject != null)
        {
            Vector3 worldPos = currentObject.transform.position + offset2;
            progressImage.transform.position = worldPos;

            // Billboard
            progressImage.transform.LookAt(Camera.main.transform);
            progressImage.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    void FollowPlayer()
    {
        if (panel != null && player != null)
        {
            Vector3 worldPos = player.position + offset;
            panel.transform.position = worldPos;

            panel.transform.LookAt(Camera.main.transform);
            panel.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    public void ShowIngredients(GameObject obj)
    {
        if (obj == null)
        {
            Clear();
            return;
        }

        int layer = obj.layer;
        if (layer == LayerMask.NameToLayer("Deer") || layer == LayerMask.NameToLayer("Wolf"))
        {
            Clear();
            return;
        }

        currentObject = obj;
        currentIngredient = obj.GetComponent<Ingredient>();

        if (currentIngredient != null)
        {
            panel.SetActive(true);
            ingredientText.text = "";

            foreach (var item in currentIngredient.ingredients)
                ingredientText.text += $"<sprite name={item.type}>  ";

            UpdateProgress(0f);
        }
    }

    void HandleHarvestInput()
    {
        if (currentIngredient == null) return;

        if (Input.GetKey(KeyCode.E))
        {
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;
            }

            holdTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(holdTimer / currentIngredient.harvestTime);
            UpdateProgress(progress);

            if (holdTimer >= currentIngredient.harvestTime)
            {
                CollectResource();
                ResetHold();
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            ResetHold();
        }
    }

    void CollectResource()
    {
        if (currentIngredient == null) return;

        // 1️⃣ Thêm tài nguyên vào IngridientManager
        if (ingredientManager != null)
        {
            foreach (var item in currentIngredient.ingredients)
            {
                ingredientManager.AddIngredient(item.type, item.quantity);
            }

            // Cập nhật UI tổng quát
            ingredientManager.DisplayResources();
        }

        // 2️⃣ Ẩn object (không destroy)
        if (currentObject != null)
        {
            currentObject.SetActive(false);
        }

        // 3️⃣ Reset target UI
        Clear();
    }

    void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;
        UpdateProgress(0f);
    }

    public void UpdateProgress(float value)
    {
        if (progressImage != null)
            progressImage.fillAmount = value;
    }

    public void Clear()
    {
        currentObject = null;
        currentIngredient = null;
        isHolding = false;
        holdTimer = 0f;
        if (panel != null) panel.SetActive(false);
        UpdateProgress(0f);
    }
}
