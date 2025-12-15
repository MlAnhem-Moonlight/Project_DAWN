using UnityEngine;
using System;

public class BuildConstruction : MonoBehaviour
{
    public enum BuildingType
    {
        Fortress,
        Watchtown,
        Barrack,
        VillagerHouse,
        Tavern
    }

    [Header("Building Settings")]
    public BuildingType buildingType;
    public bool isBuilt = false;
    public GameObject construction;
    public int constructionHP = 100;

    [Header("Loaded Resource Data")]
    public ResourceDataField T1_Total;
    public UpgradeDataField upgradeCost;

    [Header("UX Prefab")]
    public GameObject buildAvailableUI;
    public GameObject buildingActionUI;
    public GameObject smokeUI;
    public GameObject canvasPanel;
    public Vector3 uiOffset = new Vector3(0, 1f, 0);

    private BuildingResourceFileField allResources;
    private IngridientManager ingredientManager;
    private bool isPlayerInRange = false;

    [Header("Construction Manager")]
    public Transform constructionManager;


    void Start()
    {
        LoadBuildingResource();
        ingredientManager = FindFirstObjectByType<IngridientManager>();

        if (buildAvailableUI != null) buildAvailableUI.SetActive(false);
        if (buildingActionUI != null) buildingActionUI.SetActive(false);
        if (smokeUI != null && isBuilt==true) smokeUI.SetActive(false);
    }

    void Update()
    {


        if (!isBuilt)
        {
            // 🟩 Kiểm tra UI "có thể xây" khi chưa xây
            UpdateBuildAvailabilityUI();
        }
        else
        {
            // 🟩 Khi đã xây, ẩn UI "Có thể xây"
            if (buildAvailableUI != null)
                buildAvailableUI.SetActive(false);
        }

        // 🟩 BẤM E KHI ĐỨNG TRONG VÙNG VÀ CHƯA XÂY
        if (isPlayerInRange && !isBuilt && Input.GetKeyDown(KeyCode.E))
        {
            TryBuild();
        }
        else if(isPlayerInRange && isBuilt && Input.GetKeyDown(KeyCode.E) && canvasPanel != null)
        {
            canvasPanel.SetActive(true);
        }

        // 🟩 Khi công trình bị phá hủy
        if (isBuilt && constructionHP <= 0)
        {
            if (construction != null)
                construction.SetActive(false);
            if (smokeUI != null && !smokeUI.activeSelf)
                smokeUI.SetActive(true);

            GetComponent<SpriteRenderer>().enabled = true;
            isBuilt = false;

            if (buildAvailableUI != null)
                buildAvailableUI.SetActive(false);
        }
    }

    // 🟩 Khi Player vào vùng
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // 🟩 Chỉ bật Action UI khi chưa xây công trình
            if (!isBuilt && buildingActionUI != null)
            {
                UpdateUIPosition();
                buildingActionUI.SetActive(buildAvailableUI.activeSelf);
            }
        }
    }

    // 🟩 Khi Player rời vùng
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (buildingActionUI != null)
                buildingActionUI.SetActive(false);
        }
    }

    // 🟩 Load dữ liệu Resource theo loại công trình
    void LoadBuildingResource()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("building_resources");
        if (jsonFile == null)
        {
            Debug.LogError("Không tìm thấy file building_resources.json");
            return;
        }

        allResources = JsonUtility.FromJson<BuildingResourceFileField>(jsonFile.text);
        BuildingDataField selected = buildingType switch
        {
            BuildingType.Fortress => allResources.Fortress,
            BuildingType.Watchtown => allResources.Watchtown,
            BuildingType.Barrack => allResources.Barrack,
            BuildingType.VillagerHouse => allResources.VillagerHouse,
            BuildingType.Tavern => allResources.Tavern,
            _ => null
        };

        T1_Total = selected.T1_Total;
        upgradeCost = selected.Upgrade;
    }

    // 🟩 Kiểm tra UI "có thể xây"
    void UpdateBuildAvailabilityUI()
    {
        if (ingredientManager == null || buildAvailableUI == null) return;

        // ❌ Chưa có Fortress thì khóa
        if (!CanBuildByFortressRule())
        {
            buildAvailableUI.SetActive(false);
            
            return;
        }

        bool canBuild =
            ingredientManager.GetIngredientAmount("wood") >= T1_Total.wood &&
            ingredientManager.GetIngredientAmount("stone") >= T1_Total.stone &&
            ingredientManager.GetIngredientAmount("iron") >= T1_Total.iron &&
            ingredientManager.GetIngredientAmount("gold") >= T1_Total.gold &&
            ingredientManager.GetIngredientAmount("meat") >= T1_Total.meat;

        buildAvailableUI.SetActive(canBuild);
    }


    // 🟩 Cập nhật vị trí UI
    void UpdateUIPosition()
    {
        if (buildingActionUI != null)
        {
            Vector3 worldPos = transform.position + uiOffset;
            buildingActionUI.GetComponent<RectTransform>().position = worldPos;

            // Quay UI về phía camera
            buildingActionUI.transform.LookAt(Camera.main.transform);
            buildingActionUI.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    // 🟩 Xây công trình
    void TryBuild()
    {
        if (!CanBuildByFortressRule())
        {
            Debug.Log("Cần xây Fortress trước!");
            return;
        }

        bool success =
            ingredientManager.RemoveIngredient("wood", T1_Total.wood) &&
            ingredientManager.RemoveIngredient("stone", T1_Total.stone) &&
            ingredientManager.RemoveIngredient("iron", T1_Total.iron) &&
            ingredientManager.RemoveIngredient("gold", T1_Total.gold) &&
            ingredientManager.RemoveIngredient("meat", T1_Total.meat);

        if (success)
        {
            isBuilt = true;
            constructionHP = 100;

            if (construction != null)
                construction.SetActive(true);

            GetComponent<SpriteRenderer>().enabled = false;
            if (smokeUI != null) smokeUI.SetActive(false);

            if (buildAvailableUI != null)
                buildAvailableUI.SetActive(false);

            // 🟩 Tắt Action UI sau khi xây thành công
            if (buildingActionUI != null)
                buildingActionUI.SetActive(false);
        }
        else
            Debug.Log("Không đủ tài nguyên!");
    }

    bool IsFortressBuilt()
    {
        if (constructionManager == null) return false;

        foreach (Transform child in constructionManager)
        {
            BuildConstruction bc = child.GetComponent<BuildConstruction>();
            if (bc != null && bc.buildingType == BuildingType.Fortress && bc.isBuilt)
            {
                return true;
            }
        }
        return false;
    }
    bool CanBuildByFortressRule()
    {
        // Watchtown luôn được phép xây
        if (buildingType == BuildingType.Watchtown)
            return true;

        // Fortress tự nó không cần điều kiện
        if (buildingType == BuildingType.Fortress)
            return true;

        // Các công trình khác cần Fortress đã xây
        return IsFortressBuilt();
    }

}
