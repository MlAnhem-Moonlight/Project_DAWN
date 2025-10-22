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

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector2 mousePos = cam.ScreenToWorldPoint(mouseScreen);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            ArcherBehavior unit = hit.collider.GetComponent<ArcherBehavior>();
            if (unit != null)
            {
                selectedUnit = unit;
                Debug.Log($"✅ Selected: {unit.name}");
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

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        Vector2 mousePos = cam.ScreenToWorldPoint(mouseScreen);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            selectedUnit.SetCheckpoint(hit.point);
            Debug.Log($"{selectedUnit.name} moving to {hit.point}");
        }
        else
        {
            selectedUnit.SetCheckpoint(mousePos);
            Debug.Log($"{selectedUnit.name} moving to {mousePos} (no collider hit)");
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
}
