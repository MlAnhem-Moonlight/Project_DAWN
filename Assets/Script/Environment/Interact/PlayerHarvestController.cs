using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))] // make sure player has trigger collider
public class PlayerHarvestController : MonoBehaviour
{
    [Header("Interaction")]
    public float holdSeconds;
    public GameObject uiObject; // Reference to existing UI object on canvas (not prefab)
    public float interactionRadius = 1.5f; // optional if using Physics2D.OverlapCircle fallback

    [Header("Selection Visual")]
    public GameObject selectionIndicatorPrefab; // Visual indicator for selected target

    private List<Harvestable> nearby = new List<Harvestable>();
    private Harvestable currentTarget;
    private int currentTargetIndex = -1; // Index of currently selected target

    private GameObject selectionIndicator;
    private Image fillImage;
    private float holdTimer = 0f;
    private bool isHarvesting = false;

    private void Awake()
    {
        // Get the fill image from the existing UI object
        if (uiObject != null)
        {
            fillImage = uiObject.GetComponentInChildren<Image>();
            uiObject.SetActive(false); // Hide initially
        }
    }

    //If you use player trigger(recommended) :
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Trigger entered: {other.name}");
        var h = other.GetComponent<Harvestable>();
        if (h != null && !nearby.Contains(h) && !h.isHarvested)
        {
            //Debug.Log($"Harvestable entered: {h.name}");
            nearby.Add(h);

            // Nếu chưa có target nào được chọn, tự động chọn cái đầu tiên
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
            nearby.Remove(h);

            // Nếu target bị remove là target hiện tại
            if (currentTarget == h)
            {
                ClearTarget();
                // Tự động chọn target khác nếu còn
                if (nearby.Count > 0)
                {
                    int newIndex = Mathf.Min(removedIndex, nearby.Count - 1);
                    SelectTarget(newIndex);
                }
            }
            else if (removedIndex < currentTargetIndex)
            {
                // Điều chỉnh index nếu item bị remove ở trước current target
                currentTargetIndex--;
            }
        }
    }

    private void Update()
    {
        // Nếu bạn không dùng trigger collider trên player, bạn có thể uncomment kiểm tra bằng OverlapCircle:
         //UpdateNearbyWithOverlap();

        // Làm sạch danh sách nearby (loại bỏ null hoặc đã harvest)
        CleanupNearbyList();

        // Xử lý input để chuyển đổi target
        HandleTargetSelection();

        UpdateUIPosition();
        UpdateSelectionIndicator();

        // Thu hoạch với phím E
        if (currentTarget != null && !isHarvesting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                holdSeconds = currentTarget.harvestTime; // lấy thời gian thu hoạch từ target
                //Debug.Log($"Starting harvest on: {currentTarget.name} for {holdSeconds} seconds");

                if (holdSeconds > 0)
                {
                    StartCoroutine(HarvestRoutine());
                }
                else
                {
                    // Thu hoạch tức thì
                    currentTarget.CompleteHarvest();
                    CleanupNearbyList();
                }
            }
        }
    }

    private void CleanupNearbyList()
    {
        // Loại bỏ các object null hoặc đã được harvest
        for (int i = nearby.Count - 1; i >= 0; i--)
        {
            if (nearby[i] == null || nearby[i].isHarvested)
            {
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

        // Nếu không còn target nào, clear selection
        if (nearby.Count == 0)
        {
            ClearTarget();
        }
        // Nếu current index vượt quá giới hạn, điều chỉnh
        else if (currentTargetIndex >= nearby.Count)
        {
            SelectTarget(0);
        }
    }

    private void HandleTargetSelection()
    {
        if (nearby.Count <= 1) return; // Không cần chuyển đổi nếu chỉ có 1 hoặc 0 target

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Chuyển đến target tiếp theo
            int nextIndex = (currentTargetIndex + 1) % nearby.Count;
            SelectTarget(nextIndex);
        }
    }

    private void SelectTarget(int index)
    {
        if (index < 0 || index >= nearby.Count) return;

        currentTargetIndex = index;
        currentTarget = nearby[index];

        Debug.Log($"Selected target: {currentTarget.name}");
    }

    // Option: dùng Physics2D.OverlapCircle để cập nhật nearby nếu bạn không muốn trigger collider
    private void UpdateNearbyWithOverlap()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        nearby.Clear();
        foreach (var c in cols)
        {
            var h = c.GetComponent<Harvestable>();
            if (h != null && !h.isHarvested) nearby.Add(h);
        }

        // Reset target selection khi update bằng overlap
        if (nearby.Count > 0)
        {
            SelectTarget(0);
        }
        else
        {
            ClearTarget();
        }
    }

    private void UpdateUIPosition()
    {
        if (currentTarget == null)
        {
            HideUI();
            return;
        }

        // Sử dụng UI object có sẵn thay vì tạo mới
        if (uiObject != null && currentTarget != null)
        {
            Canvas canvas = uiObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform uiRect = uiObject.GetComponent<RectTransform>();

            // Get active camera (works with Cinemachine)
            Camera activeCamera = GetActiveCamera();

            // Convert world position to screen position
            Vector3 worldPos = currentTarget.GetUIWorldPosition() + Vector3.up * 0.1f;
            Vector3 screenPos = activeCamera.WorldToScreenPoint(worldPos);

            // Check if target is behind camera or outside screen
            if (screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width ||
                screenPos.y < 0 || screenPos.y > Screen.height)
            {
                uiObject.SetActive(false);
                return;
            }

            // Convert screen position to canvas position
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : activeCamera,
                out canvasPos
            );

            // Set the anchored position
            uiRect.anchoredPosition = canvasPos;

            // Show UI
            uiObject.SetActive(true);
        }
    }

    private Camera GetActiveCamera()
    {
        // Try to get Cinemachine Brain camera first
        var cinemachineBrain = UnityEngine.Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
        if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
        {
            return cinemachineBrain.OutputCamera;
        }

        // Fallback to Camera.main
        if (Camera.main != null)
        {
            return Camera.main;
        }

        // Last resort: find any active camera
        Camera mainCam = null;
        Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in cameras)
        {
            if (cam.CompareTag("MainCamera"))
            {
                mainCam = cam;
                break;
            }
        }
        foreach (Camera cam in cameras)
        {
            if (cam.isActiveAndEnabled)
            {
                return cam;
            }
        }

        // If no camera found, return null (should not happen in normal cases)
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
        {
            selectionIndicator = Instantiate(selectionIndicatorPrefab);
        }

        if (selectionIndicator != null && currentTarget != null)
        {
            selectionIndicator.transform.position = currentTarget.transform.position;
            selectionIndicator.SetActive(true);
        }
    }

    private void HideUI()
    {
        if (uiObject != null)
        {
            uiObject.SetActive(false);
        }

        // Reset fill amount
        ResetProgress();
    }

    private void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetIndex = -1;
        ResetProgress();
        HideUI();
        HideSelectionIndicator();
    }

    private IEnumerator HarvestRoutine()
    {
        if (currentTarget == null || currentTarget.isHarvested || isHarvesting) yield break;

        isHarvesting = true;
        // KHÔNG set isHarvested = true ở đây, chỉ đánh dấu đang harvest

        holdTimer = 0f;
        while (holdTimer < holdSeconds)
        {
            // nếu target bị null hoặc disabled thì abort
            if (currentTarget == null) break;

            // nếu nhả phím E thì reset và abort
            if (!Input.GetKey(KeyCode.E))
            {
                isHarvesting = false;
                ResetProgress();
                yield break;
            }

            holdTimer += Time.deltaTime;
            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(holdTimer / holdSeconds);
            }

            yield return null;
        }

        // Hoàn thành harvest
        if (currentTarget != null)
        {
            currentTarget.CompleteHarvest(); // Chỉ set isHarvested = true ở đây
        }

        // cleanup
        holdTimer = 0f;
        isHarvesting = false;
        ResetProgress();

        // remove from nearby list if it's deactivated/harvested
        CleanupNearbyList();
    }

    private void ResetProgress()
    {
        holdTimer = 0f;
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }

    private void OnDisable()
    {
        HideUI();
        HideSelectionIndicator();
    }

    private void OnDestroy()
    {
        if (selectionIndicator != null)
        {
            Destroy(selectionIndicator);
        }
    }

    // Gizmo để debug radius nếu dùng OverlapCircle
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        // Vẽ line tới current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}