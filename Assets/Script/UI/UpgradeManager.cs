using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private GameObject selectedAlly;
    private Stats selectedStats;
    private UnitUpgradeLevel selectedUpgradeCost;
    private IngridientManager resourceManager;
    private AllyCard currentAllyCard;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        resourceManager = FindAnyObjectByType<IngridientManager>();
    }

    // ============================
    // AllyCard sẽ gọi hàm này
    // ============================
    public void SetSelectedAlly(GameObject ally, Stats stats, UnitUpgradeLevel cost, AllyCard allycard)
    {
        selectedAlly = ally;
        selectedStats = stats;
        selectedUpgradeCost = cost;
        currentAllyCard = allycard;
    }

    // ============================
    // Hàm này gán vào nút Upgrade
    // ============================
    public void TryUpgrade()
    {
        if (selectedAlly == null || selectedStats == null || selectedUpgradeCost == null)
        {
            Debug.LogError("UpgradeManager: Không có ally được chọn!");
            return;
        }

        // Kiểm tra đủ tài nguyên
        if (!HasEnoughResources())
        {
            Debug.Log("Không đủ tài nguyên!");
            return;
        }

        // Trừ tài nguyên
        PayResources();

        // Tăng level
        selectedStats.level++;
        selectedStats.ApplyGrowth();

        Debug.Log(selectedAlly.name + " đã được nâng cấp!");

        // Cập nhật lại UI
        AllyCardSelectedUIRefresh();
    }

    private bool HasEnoughResources()
    {
        return resourceManager.GetIngredientAmount("wood") >= selectedUpgradeCost.wood &&
               resourceManager.GetIngredientAmount("stone") >= selectedUpgradeCost.stone &&
               resourceManager.GetIngredientAmount("iron") >= selectedUpgradeCost.iron &&
               resourceManager.GetIngredientAmount("gold") >= selectedUpgradeCost.gold;
    }

    private void PayResources()
    {
        resourceManager.RemoveIngredient("wood", selectedUpgradeCost.wood);
        resourceManager.RemoveIngredient("stone", selectedUpgradeCost.stone);
        resourceManager.RemoveIngredient("iron", selectedUpgradeCost.iron);
        resourceManager.RemoveIngredient("gold", selectedUpgradeCost.gold);
    }

    // AllyCard phải có hàm refresh UI sau khi upgrade
    private void AllyCardSelectedUIRefresh()
    {
        AllyCard currentCard = currentAllyCard;
        if (currentCard != null)
        {
            Debug.Log("Cập nhật UI cho " + currentCard.ally.name);
            //currentCard.UpgradeAlly();
            currentCard.DisplayAlly();
            currentCard.SelectedCard();
        }
    }
}
