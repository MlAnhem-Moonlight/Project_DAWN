using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private ArcherBehavior selectedUnit;
    private Stats unitAlly;


    [Header("Visual Settings")]
    public Canvas uiCanvas;              // Canvas chính (Screen Space - Overlay)
    public GameObject unitUIPrefab;      // Prefab UI hiển thị khi chọn unit (VD: Image)
    private GameObject unitUIInstance;   // Instance tạm thời của UI hiển thị

    [Header("HUD")]
    public GameObject UnitControllHUD;
    public Image AllyPortrait;
    public TextMeshProUGUI AllyName;
    public TextMeshProUGUI AllyLevel;
    public TextMeshProUGUI AllyHP;
    public TextMeshProUGUI AllyDMG;
    public TextMeshProUGUI AllyShield;
    public TextMeshProUGUI AllySkillDMG;
    public TextMeshProUGUI AllySkillCD;
    public TextMeshProUGUI AllySPD;


    void Update()
    {
        // Chọn unit bằng click chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            SelectUnit();
            if (selectedUnit != null)
            {
                AllyPortrait.sprite = unitAlly.portrait;
                AllyName.text = unitAlly.name.Replace("(Clone)", "").Trim(); ;
                AllyLevel.text = "Level: " + unitAlly.level;
                AllyHP.text = "<sprite name=\"Hp_Icon\">" + unitAlly.currentHP;
                AllyDMG.text = "<sprite name=\"Dmg_Icon\">" + unitAlly.currentDMG;
                AllyShield.text = "<sprite name=\"Shield_Icon\">" + unitAlly.currentShield;
                AllySkillDMG.text = "<sprite name=\"SkillDmg_Icon\">" + unitAlly.currentSkillDmg;
                AllySkillCD.text = "<sprite name=\"SkillCD_Icon\">" + unitAlly.currentSkillCD;
                AllySPD.text = "<sprite name=\"Spd_Icon\">" + unitAlly.currentSPD;
            }
            //else
            //{
            //    HideUnitUI();
            //    if (UnitControllHUD.activeSelf)
            //        UnitControllHUD.SetActive(false);
            //}

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

            if (Input.GetMouseButtonDown(1))
            {
                CommandMove();
            }
        }

        // Ra lệnh di chuyển bằng click chuột phải

    }

    private void SelectUnit()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No MainCamera found!");
            return;
        }

        Vector3 worldPoint = ScreenToWorldPointOnPlane(Input.mousePosition, cam, 0f);

        // Lấy tất cả collider tại điểm click
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

        if (hits.Length == 0)
            return;

        Stats chosenUnit = null;
        ArcherBehavior chosenArcher = null;

        // Lọc collider có Stats
        foreach (var h in hits)
        {
            Stats s = h.GetComponent<Stats>();
            if (s != null)
            {
                chosenUnit = s;

                // Lấy ArcherBehavior nếu có (không bắt buộc)
                chosenArcher = h.GetComponent<ArcherBehavior>();
                break; // lấy đúng unit đầu tiên có Stats
            }
        }

        if (chosenUnit != null)
        {
            unitAlly = chosenUnit;

            if (!UnitControllHUD.activeSelf)
                UnitControllHUD.SetActive(true);

            // Gán archer nếu có
            if (chosenArcher != null)
                selectedUnit = chosenArcher;
        }


    }


    public void CommandMove()
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

    public void ShowUnitUI()
    {
        if (unitUIPrefab == null || uiCanvas == null) return;

        if (unitUIInstance != null)
            Destroy(unitUIInstance);

        unitUIInstance = Instantiate(unitUIPrefab, uiCanvas.transform);
        unitUIInstance.name = $"{selectedUnit.name}_UI";
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
