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
    public Vector3 uiOffset = new Vector3(0, 1f, 0);

    private BuildingResourceFileField allResources;
    private IngridientManager ingredientManager;
    private bool isPlayerInRange = false;

    void Start()
    {
        LoadBuildingResource();
        ingredientManager = FindFirstObjectByType<IngridientManager>();

        if (buildAvailableUI != null) buildAvailableUI.SetActive(false);
        if (buildingActionUI != null) buildingActionUI.SetActive(false);
    }

    void Update()
    {

        // Cập nhật UI có thể xây
        if (isBuilt == false)
            UpdateBuildAvailabilityUI();
        else
        {
            if (buildAvailableUI != null) buildAvailableUI.SetActive(false);
            if (buildingActionUI != null) buildingActionUI.SetActive(false);
        }

        // 🟩 BẤM E KHI ĐỨNG TRONG VÙNG
        if (isPlayerInRange && !isBuilt && Input.GetKeyDown(KeyCode.E))
        {
            TryBuild();
        }

        if (isBuilt == true && constructionHP <= 0)
        {
            if (construction != null)
                construction.SetActive(false);

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
            if (buildingActionUI != null)
            {
                UpdateUIPosition();
                buildingActionUI.SetActive(true);
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

        bool canBuild =
            ingredientManager.GetIngredientAmount("wood") >= T1_Total.wood &&
            ingredientManager.GetIngredientAmount("stone") >= T1_Total.stone &&
            ingredientManager.GetIngredientAmount("iron") >= T1_Total.iron &&
            ingredientManager.GetIngredientAmount("gold") >= T1_Total.gold &&
            ingredientManager.GetIngredientAmount("meat") >= T1_Total.meat;

        buildAvailableUI.SetActive(canBuild);
    }

    void UpdateUIPosition()
    {

        if (buildingActionUI != null)
        {
            Vector3 worldPos = transform.position + uiOffset;
            buildingActionUI.transform.position = worldPos;

            buildingActionUI.transform.LookAt(Camera.main.transform);
            buildingActionUI.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    // 🟩 Xây công trình
    void TryBuild()
    {
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

            if (buildAvailableUI != null)
                buildAvailableUI.SetActive(false);
        }
        else
            Debug.Log("Không đủ tài nguyên!");
    }
}