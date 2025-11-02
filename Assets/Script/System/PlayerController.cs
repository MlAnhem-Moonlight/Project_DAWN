using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private ArcherBehavior selectedUnit;
    [Header("Visual Settings")]
    public Canvas uiCanvas;              // Canvas chính (Screen Space - Overlay)
    public GameObject unitUIPrefab;      // Prefab UI hiển thị khi chọn unit (VD: Image)
    private GameObject unitUIInstance;   // Instance tạm thời của UI hiển thị

    void Update()
    {
        // Chọn unit bằng click chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            SelectUnit();
        }

        // Khi đã chọn, cho phép UI theo chuột
        if (unitUIInstance != null)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.transform as RectTransform,
                Input.mousePosition,
                uiCanvas.worldCamera,
                out mousePos);
            unitUIInstance.GetComponent<RectTransform>().anchoredPosition = mousePos;
        }

        // Ra lệnh di chuyển bằng click chuột phải
        if (Input.GetMouseButtonDown(1))
        {
            CommandMove();
        }
    }

    private void SelectUnit()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No MainCamera found!");
            return;
        }

        Vector3 worldPoint = ScreenToWorldPointOnPlane(Input.mousePosition, cam, 0f); // planeZ = 0 (ground)
        // Dùng OverlapPoint cho 2D, chính xác hơn khi click vào collider 2D
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint);

        if (hitCollider != null)
        {
            ArcherBehavior unit = hitCollider.GetComponent<ArcherBehavior>();
            if (unit != null)
            {
                selectedUnit = unit;
                //Debug.Log($"✅ Selected: {unit.name}");
                ShowUnitUI(unit); // Hiển thị biểu tượng UI khi chọn
            }
        }
    }

    private void CommandMove()
    {
        if (selectedUnit == null) return;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        // Lấy world point trên cùng plane z với unit để tránh chênh z
        float planeZ = selectedUnit.transform.position.z; // giữ z của unit
        Vector3 worldPoint = ScreenToWorldPointOnPlane(Input.mousePosition, cam, planeZ);

        // Tìm collider tại điểm đó (nếu có)
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint);

        if (hitCollider != null)
        {
            selectedUnit.SetCheckpoint((Vector2)worldPoint);
            //Debug.Log($"{selectedUnit.name} moving to {worldPoint} (hit collider: {hitCollider.name})");
        }
        else
        {
            selectedUnit.SetCheckpoint((Vector2)worldPoint);
            //Debug.Log($"{selectedUnit.name} moving to {worldPoint} (no collider hit)");
        }

        // Hủy chọn sau khi ra lệnh
        selectedUnit = null;
        HideUnitUI();
    }

    private void ShowUnitUI(ArcherBehavior unit)
    {
        if (unitUIPrefab == null || uiCanvas == null) return;

        if (unitUIInstance != null)
            Destroy(unitUIInstance);

        unitUIInstance = Instantiate(unitUIPrefab, uiCanvas.transform);
        unitUIInstance.name = $"{unit.name}_UI";
        unitUIInstance.SetActive(true);
    }

    private void HideUnitUI()
    {
        if (unitUIInstance != null)
        {
            Destroy(unitUIInstance);
            unitUIInstance = null;
        }
    }

    /// <summary>
    /// Convert screen point -> world point nằm trên plane z = planeZ (hỗ trợ cả orthographic & perspective)
    /// </summary>
    private Vector3 ScreenToWorldPointOnPlane(Vector3 screenPos, Camera cam, float planeZ)
    {
        // Nếu camera orthographic thì z không ảnh hưởng, dùng ScreenToWorldPoint mặc định
        if (cam.orthographic)
        {
            Vector3 wp = cam.ScreenToWorldPoint(screenPos);
            wp.z = planeZ;
            return wp;
        }

        // Với perspective camera: cần truyền distance từ camera tới planeZ
        float distance = Mathf.Abs(cam.transform.position.z - planeZ);
        Vector3 sp = new Vector3(screenPos.x, screenPos.y, distance);
        Vector3 worldPoint = cam.ScreenToWorldPoint(sp);
        worldPoint.z = planeZ;
        return worldPoint;
    }
}
