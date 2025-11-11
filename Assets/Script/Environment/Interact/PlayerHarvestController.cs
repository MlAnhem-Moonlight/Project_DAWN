using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class PlayerHarvestController : MonoBehaviour
{
    [Header("Interaction")]
    public float holdSeconds;
    public GameObject uiObject;
    public float interactionRadius = 1.5f;

    [Header("Selection Visual")]
    public GameObject selectionIndicatorPrefab;
    public GameObject actionPrefab; // ⚡ Prefab hiển thị action (vd: icon phím E, nút hành động)
    private GameObject actionInstance; // instance đang hiển thị
    public Vector3 actionOffset = new Vector3(0, 1f, 0); // khoảng cách hiển thị trên đầu target

    [Header("Material Highlighting")]
    public Material highlightMaterial;
    public Color targetColor = Color.yellow;
    public string colorPropertyName = "_Color";

    private List<Harvestable> nearby = new List<Harvestable>();
    private Harvestable currentTarget;
    private int currentTargetIndex = -1;

    private GameObject selectionIndicator;
    private Image fillImage;
    private float holdTimer = 0f;
    private bool isHarvesting = false;

    private Dictionary<Harvestable, Material> originalMaterials = new Dictionary<Harvestable, Material>();
    private Dictionary<Harvestable, Color> originalColors = new Dictionary<Harvestable, Color>();

    private void Awake()
    {
        if (uiObject != null)
        {
            fillImage = uiObject.GetComponentInChildren<Image>();
            uiObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var h = other.GetComponent<Harvestable>();
        if (h != null && !nearby.Contains(h) && !h.isHarvested)
        {
            nearby.Add(h);
            SaveOriginalMaterial(h);

            if (currentTarget == null)
            {
                SelectTarget(0);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var h = other.GetComponent<Harvestable>();
        if (h != null && nearby.Contains(h))
        {
            int removedIndex = nearby.IndexOf(h);
            RestoreOriginalMaterial(h);
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
            {
                currentTargetIndex--;
            }
        }
    }

    private void Update()
    {
        CleanupNearbyList();
        HandleTargetSelection();

        UpdateUIPosition();
        UpdateSelectionIndicator();
        UpdateActionPrefab(); // ⚡ update vị trí & trạng thái prefab hành động

        if (currentTarget != null && !isHarvesting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                holdSeconds = currentTarget.harvestTime;
                if (holdSeconds > 0)
                {
                    StartCoroutine(HarvestRoutine());
                }
                else
                {
                    currentTarget.CompleteHarvest();
                    CleanupNearbyList();
                }
            }
        }
    }

    private void HandleTargetSelection()
    {
        if (nearby.Count <= 1) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var oldTarget = currentTarget;
            int nextIndex = (currentTargetIndex + 1) % nearby.Count;
            if (oldTarget != null)
            {
                RestoreOriginalMaterial(oldTarget);
            }
            SelectTarget(nextIndex);
        }
    }

    private void SelectTarget(int index)
    {
        if (index < 0 || index >= nearby.Count) return;

        currentTargetIndex = index;
        currentTarget = nearby[index];

        SaveOriginalMaterial(currentTarget);
        ApplyHighlightMaterial(currentTarget);

        ShowActionPrefab(currentTarget); // ⚡ tạo prefab hành động
    }

    private void ShowActionPrefab(Harvestable target)
    {
        if (actionPrefab == null) return;

        // Xóa instance cũ nếu có
        if (actionInstance != null)
            Destroy(actionInstance);

        // Tìm Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("⚠️ Không tìm thấy Canvas để hiển thị action prefab!");
            return;
        }

        // Tạo instance
        actionInstance = Instantiate(actionPrefab, canvas.transform);
        RectTransform rect = actionInstance.GetComponent<RectTransform>();

        if (rect == null)
        {
            Debug.LogWarning("⚠️ Action prefab không có RectTransform (không phải UI prefab?)");
            return;
        }

        UpdateActionPrefabPosition(target, rect, canvas);
    }

    private void UpdateActionPrefab()
    {
        if (actionInstance == null || currentTarget == null || transform == null) return;

        Canvas canvas = actionInstance.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform rect = actionInstance.GetComponent<RectTransform>();
        UpdateActionPrefabPosition(currentTarget, rect, canvas);
    }

    private void UpdateActionPrefabPosition(Harvestable target, RectTransform rect, Canvas canvas)
    {
        Camera cam = GetActiveCamera();
        if (cam == null) return;

        // ✅ Tính vị trí trung gian giữa Player và Target
        Vector3 targetPos = target.transform.position + actionOffset;
        Vector3 playerPos = transform.position;
        Vector3 midPos = Vector3.Lerp(targetPos, playerPos, 0.3f); // 0.3f: nghiêng 30% về phía Player

        // Convert sang toạ độ màn hình
        Vector3 screenPos = cam.WorldToScreenPoint(midPos);

        // Ẩn nếu phía sau camera
        if (screenPos.z < 0)
        {
            actionInstance.SetActive(false);
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out canvasPos
        );

        rect.anchoredPosition = canvasPos;
        actionInstance.SetActive(true);
    }


    private void HideActionPrefab()
    {
        if (actionInstance != null)
        {
            Destroy(actionInstance);
            actionInstance = null;
        }
    }

    private void SaveOriginalMaterial(Harvestable harvestable)
    {
        var renderer = harvestable.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            if (!originalMaterials.ContainsKey(harvestable))
                originalMaterials[harvestable] = renderer.material;
            if (renderer.material.HasProperty(colorPropertyName) && !originalColors.ContainsKey(harvestable))
                originalColors[harvestable] = renderer.material.GetColor(colorPropertyName);
        }
    }

    private void ApplyHighlightMaterial(Harvestable harvestable)
    {
        var renderer = harvestable.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (highlightMaterial != null)
            {
                renderer.material = highlightMaterial;
                if (renderer.material.HasProperty(colorPropertyName))
                    renderer.material.SetColor(colorPropertyName, targetColor);
            }
            else if (renderer.material.HasProperty(colorPropertyName))
                renderer.material.SetColor(colorPropertyName, targetColor);
        }
    }

    private void RestoreOriginalMaterial(Harvestable harvestable)
    {
        var renderer = harvestable.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (originalMaterials.ContainsKey(harvestable))
            {
                renderer.material = originalMaterials[harvestable];
                originalMaterials.Remove(harvestable);
            }
            else if (originalColors.ContainsKey(harvestable))
            {
                if (renderer.material.HasProperty(colorPropertyName))
                    renderer.material.SetColor(colorPropertyName, originalColors[harvestable]);
            }

            if (originalColors.ContainsKey(harvestable))
                originalColors.Remove(harvestable);
        }
    }

    private void CleanupNearbyList()
    {
        for (int i = nearby.Count - 1; i >= 0; i--)
        {
            if (nearby[i] == null || nearby[i].isHarvested)
            {
                RestoreOriginalMaterial(nearby[i]);

                if (i == currentTargetIndex)
                {
                    ClearTarget();
                }
                else if (i < currentTargetIndex)
                {
                    currentTargetIndex--;
                }
                nearby.RemoveAt(i);
            }
        }

        if (nearby.Count == 0)
        {
            ClearTarget();
        }
        else if (currentTargetIndex >= nearby.Count)
        {
            SelectTarget(0);
        }
    }

    private void UpdateUIPosition()
    {
        if (currentTarget == null)
        {
            HideUI();
            return;
        }

        if (uiObject != null && currentTarget != null)
        {
            Canvas canvas = uiObject.GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform uiRect = uiObject.GetComponent<RectTransform>();
            Camera activeCamera = GetActiveCamera();

            Vector3 worldPos = currentTarget.GetUIWorldPosition() + Vector3.up * 0.1f;
            Vector3 screenPos = activeCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.y < 0 || screenPos.y > Screen.height)
            {
                uiObject.SetActive(false);
                return;
            }

            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : activeCamera,
                out canvasPos
            );

            uiRect.anchoredPosition = canvasPos;
            uiObject.SetActive(true);
        }
    }

    private Camera GetActiveCamera()
    {
        var cinemachineBrain = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
        if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
            return cinemachineBrain.OutputCamera;

        if (Camera.main != null)
            return Camera.main;

        Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in cameras)
        {
            if (cam.isActiveAndEnabled)
                return cam;
        }

        Debug.LogWarning("No active camera found for UI positioning!");
        return null;
    }

    private void UpdateSelectionIndicator()
    {
        if (currentTarget == null)
        {
            HideSelectionIndicator();
            return;
        }

        if (selectionIndicator == null && selectionIndicatorPrefab != null)
            selectionIndicator = Instantiate(selectionIndicatorPrefab);

        if (selectionIndicator != null && currentTarget != null)
        {
            selectionIndicator.transform.position = currentTarget.transform.position;
            selectionIndicator.SetActive(true);
        }
    }

    private void HideUI()
    {
        if (uiObject != null)
            uiObject.SetActive(false);
        ResetProgress();
    }

    private void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
    }

    private void ClearTarget()
    {
        if (currentTarget != null)
            RestoreOriginalMaterial(currentTarget);

        currentTarget = null;
        currentTargetIndex = -1;

        HideUI();
        HideSelectionIndicator();
        HideActionPrefab(); // ⚡ ẩn prefab hành động
        ResetProgress();
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

    private void OnDisable()
    {
        foreach (var h in nearby)
        {
            if (h != null) RestoreOriginalMaterial(h);
        }

        HideUI();
        HideSelectionIndicator();
        HideActionPrefab();
    }

    private void OnDestroy()
    {
        foreach (var h in nearby)
        {
            if (h != null) RestoreOriginalMaterial(h);
        }

        if (selectionIndicator != null)
            Destroy(selectionIndicator);
        HideActionPrefab();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}
