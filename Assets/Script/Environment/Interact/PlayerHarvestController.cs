using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PlayerHarvestController : MonoBehaviour
{
    [Header("Interaction")]
    public float holdSeconds;
    public GameObject uiObject; // thanh progress
    public float interactionRadius = 1.5f;

    [Header("Ingredient Canvas (sẵn trong Canvas)")]
    [Tooltip("Drag sẵn GameObject IngredientCanvas trong Canvas vào đây (ban đầu ẩn đi).")]
    public GameObject ingredientCanvasInstance;
    public Vector3 canvasOffset = new Vector3(-1.5f, 3.5f, 0f);
    private TextMeshProUGUI ingredientText;

    private List<Harvestable> nearby = new List<Harvestable>();
    private Harvestable currentTarget;
    private int currentTargetIndex = -1;

    private Image fillImage;
    private float holdTimer = 0f;
    private bool isHarvesting = false;

    // Canvas references
    private Canvas parentCanvas;
    private RectTransform canvasRectTransform;
    private RectTransform ingredientRectTransform;

    // Static đảm bảo chỉ 1 canvas hiển thị
    private static PlayerHarvestController activeController = null;

    private void Awake()
    {
        if (uiObject != null)
        {
            fillImage = uiObject.GetComponentInChildren<Image>();
            uiObject.SetActive(false);
        }

        if (ingredientCanvasInstance != null)
        {
            ingredientText = ingredientCanvasInstance.GetComponentInChildren<TextMeshProUGUI>();
            ingredientCanvasInstance.SetActive(false);

            parentCanvas = ingredientCanvasInstance.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                canvasRectTransform = parentCanvas.GetComponent<RectTransform>();

            ingredientRectTransform = ingredientCanvasInstance.GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        CleanupNearbyList();
        HandleTargetSelection();
        UpdateUIPosition();

        // Chỉ controller đang active mới cập nhật IngredientCanvas
        if (activeController == this)
            UpdateIngredientCanvasPosition();

        if (currentTarget != null && !isHarvesting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                holdSeconds = currentTarget.harvestTime;
                if (holdSeconds > 0)
                    StartCoroutine(HarvestRoutine());
                else
                    currentTarget.CompleteHarvest();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var h = other.GetComponent<Harvestable>();
        if (h != null && !nearby.Contains(h) && !h.isHarvested)
        {
            nearby.Add(h);
            if (currentTarget == null)
                SelectTarget(0);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var h = other.GetComponent<Harvestable>();
        if (h != null && nearby.Contains(h))
        {
            int removedIndex = nearby.IndexOf(h);
            nearby.Remove(h);

            if (currentTarget == h)
            {
                ClearTarget();
                if (nearby.Count > 0)
                {
                    int newIndex = Mathf.Min(removedIndex, nearby.Count - 1);
                    SelectTarget(newIndex);
                }
            }
            else if (removedIndex < currentTargetIndex)
                currentTargetIndex--;
        }
    }

    private void HandleTargetSelection()
    {
        if (nearby.Count <= 1) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            int nextIndex = (currentTargetIndex + 1) % nearby.Count;
            SelectTarget(nextIndex);
        }
    }

    private IEnumerator HarvestRoutine()
    {
        if (currentTarget == null || currentTarget.isHarvested || isHarvesting) yield break;

        isHarvesting = true;
        holdTimer = 0f;

        while (holdTimer < holdSeconds)
        {
            if (currentTarget == null) break;
            if (!Input.GetKey(KeyCode.E))
            {
                isHarvesting = false;
                ResetProgress();
                yield break;
            }

            holdTimer += Time.deltaTime;
            if (fillImage != null)
                fillImage.fillAmount = Mathf.Clamp01(holdTimer / holdSeconds);
            yield return null;
        }

        if (currentTarget != null)
            currentTarget.CompleteHarvest();

        holdTimer = 0f;
        isHarvesting = false;
        ResetProgress();
        CleanupNearbyList();
    }

    private void ResetProgress()
    {
        holdTimer = 0f;
        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }

    private void SelectTarget(int index)
    {
        if (index < 0 || index >= nearby.Count) return;

        currentTargetIndex = index;
        currentTarget = nearby[index];
        ShowIngredientCanvas();
    }

    private void ShowIngredientCanvas()
    {
        if (ingredientCanvasInstance == null) return;

        // Ẩn canvas cũ nếu controller khác đang hiển thị
        if (activeController != null && activeController != this)
            activeController.HideIngredientCanvas();

        activeController = this;

        ingredientCanvasInstance.SetActive(true);
        UpdateIngredientCanvasPosition();

        var ingredientScript = currentTarget.GetComponent<Ingredient>();
        if (ingredientText != null && ingredientScript != null && ingredientScript.ingredients != null)
        {
            string text = "";
            foreach (var entry in ingredientScript.ingredients)
                text += $"<sprite name={entry.type}>  ";
            ingredientText.text = text;
        }
        else if (ingredientText != null)
            ingredientText.text = "Không có nguyên liệu.";
    }

    private void UpdateIngredientCanvasPosition()
    {
        if (ingredientCanvasInstance == null || !ingredientCanvasInstance.activeSelf) return;
        if (parentCanvas == null || ingredientRectTransform == null) return;

        Camera cam = GetActiveCamera();
        Vector3 worldPos = transform.position + canvasOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0)
        {
            ingredientCanvasInstance.SetActive(false);
            if (activeController == this)
                activeController = null;
            return;
        }

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out canvasPos
        );

        ingredientRectTransform.anchoredPosition = canvasPos;
    }

    private void HideIngredientCanvas()
    {
        if (ingredientCanvasInstance != null)
            ingredientCanvasInstance.SetActive(false);

        if (activeController == this)
            activeController = null;
    }

    private void UpdateUIPosition()
    {
        if (currentTarget == null)
        {
            HideUI();
            return;
        }

        if (uiObject != null)
        {
            Camera cam = GetActiveCamera();
            RectTransform canvasRect = uiObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            RectTransform uiRect = uiObject.GetComponent<RectTransform>();

            Vector3 worldPos = currentTarget.transform.position + Vector3.up * 0.1f;
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0)
            {
                uiObject.SetActive(false);
                return;
            }

            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                canvasRect.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out canvasPos
            );

            uiRect.anchoredPosition = canvasPos;
            uiObject.SetActive(true);
        }
    }

    private void HideUI()
    {
        if (uiObject != null)
            uiObject.SetActive(false);
        ResetProgress();
    }

    private void CleanupNearbyList()
    {
        for (int i = nearby.Count - 1; i >= 0; i--)
        {
            if (nearby[i] == null || nearby[i].isHarvested)
            {
                if (i == currentTargetIndex)
                    ClearTarget();
                else if (i < currentTargetIndex)
                    currentTargetIndex--;
                nearby.RemoveAt(i);
            }
        }

        if (nearby.Count == 0)
            ClearTarget();
        else if (currentTargetIndex >= nearby.Count)
            SelectTarget(0);
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetIndex = -1;
        HideUI();
        HideIngredientCanvas();
        ResetProgress();
    }

    private void OnDisable()
    {
        if (activeController == this)
            HideIngredientCanvas();
    }

    private void OnDestroy()
    {
        if (activeController == this)
            activeController = null;
    }

    private Camera GetActiveCamera()
    {
        if (Camera.main != null)
            return Camera.main;
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in cameras)
            if (cam.isActiveAndEnabled) return cam;
        return null;
    }
}
