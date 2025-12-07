using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MultiUIRandomizer : MonoBehaviour
{

    [System.Serializable]
    public class UIGroup
    {
        public RectTransform targetImage;   // vị trí/size cũ
        public GameObject[] prefabs;        // danh sách prefab cho riêng target này
        [HideInInspector] public GameObject instance; // prefab đang dùng
    }
    [Header("Random Setting")]
    [Range(0, 100)]
    public int ReRollCount = 2;
    public UIGroup[] groups;   // danh sách target + prefab list
    [Range(1, 3)]
    public int TowerLevel = 1;
    [Header("UI Stats")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statText;
    public TextMeshProUGUI unitname;
    public TextMeshProUGUI reqDetail;
    public TextMeshProUGUI log;

    [Header("Spawned Object")]
    public GameObject selected;
    public GameObject cardSelected;
    public GameObject rpUI;

    public EnemySpawner allySpawner;
    [Header("Selected Card")]
    public ShowStatsUI statsUI;   // để đọc costData từ ShowStatsUI
    public IngridientManager ingredientManager; // để trừ tài nguyên


    public void SetSelected(GameObject obj, GameObject obj1)
    {
        selected = obj;
        cardSelected = obj1;
        rpUI = cardSelected.transform.Find("OutOfStock").gameObject;
    }

    public void HireHero()
    {

        if (allySpawner != null)
        {
            if(allySpawner.CheckSpawnLimit() == false)
            {
                log.text = "No more room for this soldier!!!";
                Debug.Log("Quá tải dân số");
                return;
            }

        }
        else Debug.LogWarning("Ally Spawner is null");
        if (cardSelected == null)
        {
            Debug.LogError("Chưa chọn card để Hire!");
            return;
        }
        IngridientManager ing = FindFirstObjectByType<IngridientManager>();
        ShowStatsUI statsUI = cardSelected.GetComponent<ShowStatsUI>();

        if (ing == null || statsUI == null)
        {
            Debug.LogError("Thiếu IngridientManager hoặc ShowStatsUI");
            return;
        }

        // Lấy cost theo level của hero đang chọn
        var costInfo = statsUI.GetSelectedCost();
        UnitCostLevel cost = costInfo.levelCost;
        int meatCost = costInfo.meatCost;

        if (cost == null)
        {
            Debug.LogError("Không tìm thấy dữ liệu cost");
            return;
        }

        // Kiểm tra đủ tài nguyên chưa
        if (!ing.CheckEnough(cost, meatCost))
        {
            Debug.Log("❌ Không đủ tài nguyên để Hire!");
            return;
        }

        // Trừ tài nguyên
        ing.RemoveIngredient("wood", cost.wood);
        ing.RemoveIngredient("stone", cost.stone);
        ing.RemoveIngredient("iron", cost.iron);
        ing.RemoveIngredient("gold", cost.gold);
        ing.RemoveIngredient("meat", meatCost);

        // Spawn Hero hoặc thêm vào danh sách
        AddHero();
        Debug.Log("🟩 Thuê thành công hero!");
    }

    public void AddHero()
    {

        if (selected == null || cardSelected == null)
        {
            Debug.LogWarning("No selected object to hire");
            return;
        }

        if (allySpawner != null)
        {
            allySpawner.SpawnAlly(selected.name.Replace("(Clone)", "").Trim(), selected.GetComponent<Stats>().level);
        }
        else Debug.LogWarning("Ally Spawner is null");
        cardSelected.GetComponentInParent<Button>().interactable = false;
        rpUI.GetComponent<TextMeshProUGUI>().enabled = true;
        selected = cardSelected = rpUI = null;
    }

    void Start()
    {
        RandomizeAll();
    }

    private void Update()
    {
        if (GetComponent<UnityEngine.UI.Button>() != null)
        {
            if (ReRollCount > 0 && GetComponent<UnityEngine.UI.Button>().interactable == false)
            {

                GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }
    }

    public void ClearText()
    {
        levelText.text = "";
        statText.text = "";
        unitname.text = "";
        reqDetail.text = "";
    }

    [ContextMenu("Replace Prefab")]
    public void RandomizeAll()
    {
        if (ReRollCount > 0) ReRollCount--;

        levelText.text = "";
        statText.text = "";
        unitname.text = "";
        reqDetail.text = "";

        foreach (var group in groups)
        {
            ReplacePrefab(group);
        }
        if (ReRollCount <= 0)
        {
            Debug.LogWarning("No More Roll Left");
            if (GetComponent<UnityEngine.UI.Button>() != null)
            {
                GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
        }

    }
    public void ReplacePrefab(UIGroup group)
    {
        Debug.Log("Replacing prefab for " + group.targetImage.name);
        // Xoá prefab cũ nếu có
        if (group.instance != null)
            Destroy(group.instance);

        // Random 1 prefab từ group.prefabs
        int index = Random.Range(0, group.prefabs.Length);
        GameObject prefab = group.prefabs[index];

        // Instantiate vào đúng Canvas Parent
        group.instance = Instantiate(prefab, group.targetImage.parent);

        // Copy toàn bộ layout từ target
        RectTransform newRT = group.instance.GetComponent<RectTransform>();
        RectTransform oldRT = group.targetImage;

        newRT.anchorMin = oldRT.anchorMin;
        newRT.anchorMax = oldRT.anchorMax;
        newRT.pivot = oldRT.pivot;
        newRT.sizeDelta = oldRT.sizeDelta;
        newRT.anchoredPosition = oldRT.anchoredPosition;
        newRT.localRotation = oldRT.localRotation;
        newRT.localScale = oldRT.localScale;
        switch (TowerLevel)
        {
            case 1:
                RandomStats(prefab, 3);
                break;
            case 2:
                RandomStats(prefab, 6);
                break;
            case 3:
                RandomStats(prefab, 8);
                break;
        }


        group.targetImage.gameObject.SetActive(false); // ẩn target cũ
        group.targetImage = newRT; // cập nhật target mới

    }

    private void RandomStats(GameObject prefab, int maxLevel)
    {
        if (prefab.GetComponent<Stats>() != null)
        {
            prefab.GetComponent<Stats>().level = Random.Range(1, maxLevel);
            prefab.GetComponent<Stats>().ApplyGrowth();
        }
    }

}
