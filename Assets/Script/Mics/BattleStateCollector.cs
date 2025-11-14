using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Thu thập state từ đội hình ally và tạo state đơn giản hóa cho Q-Learning
/// Bây giờ có thêm level của người chơi
/// </summary>
public class BattleStateCollector : MonoBehaviour
{
    [Header("Player Progress")]
    [Tooltip("Level hiện tại của người chơi (1-20)")]
    [Range(1, 20)]
    public int playerLevel = 1;

    [Header("Runtime")]
    [Tooltip("State đã được đơn giản hóa để Q-Learning có thể học")]
    public string currentState = "";

    [Tooltip("Danh sách ally hiện đang tồn tại trên sân")]
    public List<Stats> activeAllies = new List<Stats>();

    [Header("State Configuration")]
    [Tooltip("Phân loại HP thành các khoảng (ví dụ: 0-50, 51-100, 101-150)")]
    public int hpBracketSize = 50;

    [Tooltip("Phân loại DMG thành các khoảng")]
    public int dmgBracketSize = 10;

    [Header("Level Brackets")]
    [Tooltip("Chia 20 level thành các tier")]
    public int levelBracketSize = 5; // Early (1-5), Mid (6-10), Late (11-15), End (16-20)

    void Update()
    {
        RefreshActiveAllies();
        currentState = BuildSimplifiedState();
    }

    /// <summary>
    /// Cập nhật danh sách ally đang tồn tại trên sân.
    /// </summary>
    [ContextMenu("Get Ally")]
    public void RefreshActiveAllies()
    {
        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag("Ally");
        activeAllies = allyObjects
            .Select(o => o.GetComponent<Stats>())
            .Where(s => s != null && s.gameObject.activeInHierarchy)
            .ToList();
    }

    /// <summary>
    /// Tạo state đơn giản hóa theo các đặc trưng tổng quát
    /// Thay vì lưu HP/DMG chính xác, ta dùng bracket và aggregation
    /// </summary>
    [ContextMenu("Gen State")]
    string BuildSimplifiedState()
    {
        if (activeAllies.Count == 0)
            return $"Empty_L{playerLevel}";

        // Đếm số lượng từng loại unit
        Dictionary<string, int> unitCounts = new Dictionary<string, int>();

        // Tính tổng power (level-weighted)
        int totalPower = 0;
        int totalLevel = 0;
        float avgHP = 0;
        float avgDMG = 0;

        foreach (var stats in activeAllies)
        {
            string type = stats.baseStats != null ? stats.baseStats.name : "Unknown";

            // Đếm số lượng
            if (!unitCounts.ContainsKey(type))
                unitCounts[type] = 0;
            unitCounts[type]++;

            // Tính power metrics
            totalLevel += stats.level;
            totalPower += stats.level * 10; // weight by level
            avgHP += stats.currentHP;
            avgDMG += stats.currentDMG;
        }

        int count = activeAllies.Count;
        avgHP /= count;
        avgDMG /= count;

        // Phân loại HP và DMG vào brackets
        int hpCategory = Mathf.FloorToInt(avgHP / hpBracketSize);
        int dmgCategory = Mathf.FloorToInt(avgDMG / dmgBracketSize);

        // Tạo composition string (sắp xếp để đảm bảo nhất quán)
        var sortedUnits = unitCounts.OrderBy(kvp => kvp.Key).ToList();
        string composition = string.Join("_", sortedUnits.Select(kvp => $"{kvp.Key}x{kvp.Value}"));

        // State format: "PlayerLevel_Composition_Count_AvgLevel_HPCategory_DMGCategory"
        string state = $"L{playerLevel}_{composition}_{count}u_AL{totalLevel / count}_H{hpCategory}_D{dmgCategory}";

        return state;
    }

    /// <summary>
    /// State đơn giản hơn nữa - dùng player level + power tier
    /// Level cao nhưng yếu → spawn enemy lv cao nhưng ít
    /// Level cao và mạnh → spawn enemy lv cao và nhiều
    /// </summary>
    public string GetSimpleState()
    {
        if (activeAllies.Count == 0)
            return $"Empty_L{playerLevel}";

        // Tính tổng power
        int totalPower = 0;
        Dictionary<string, int> unitCounts = new Dictionary<string, int>();

        foreach (var stats in activeAllies)
        {
            string type = stats.baseStats != null ? stats.baseStats.name : "Unknown";
            if (!unitCounts.ContainsKey(type))
                unitCounts[type] = 0;
            unitCounts[type]++;

            totalPower += stats.level * 10 + Mathf.RoundToInt(stats.currentHP / 10) + Mathf.RoundToInt(stats.currentDMG);
        }

        // Phân level tier (Early/Mid/Late/End)
        string levelTier = GetLevelTier(playerLevel);

        // Phân power thành tiers (Weak: 0-100, Medium: 101-200, Strong: 201+)
        string powerTier = totalPower <= 100 ? "Weak" :
                          totalPower <= 200 ? "Medium" :
                          totalPower <= 350 ? "Strong" : "VeryStrong";

        // Composition
        var sortedUnits = unitCounts.OrderBy(kvp => kvp.Key).ToList();
        string composition = string.Join("_", sortedUnits.Select(kvp => $"{kvp.Key}x{kvp.Value}"));

        // Format: LevelTier_PowerTier_Composition
        return $"{levelTier}_{powerTier}_{composition}";
    }

    /// <summary>
    /// Trả về level của enemy nên spawn (base level cho spawner)
    /// Level này phụ thuộc vào player level, không phụ thuộc vào power
    /// </summary>
    public int GetRecommendedEnemyLevel()
    {
        // Enemy level luôn scale theo player level
        // Có thể thêm variance nhỏ ±1 level
        return Mathf.Clamp(playerLevel + Random.Range(-1, 2), 1, 20);
    }

    /// <summary>
    /// Lấy level tier để group states
    /// </summary>
    private string GetLevelTier(int level)
    {
        if (level <= 5) return "Early"; // 1-5
        if (level <= 10) return "Mid";   // 6-10
        if (level <= 15) return "Late";  // 11-15
        return "End";                     // 16-20
    }

    /// <summary>
    /// Đánh giá strength của player so với level
    /// Dùng để điều chỉnh số lượng enemy
    /// </summary>
    public float GetPowerToLevelRatio()
    {
        if (activeAllies.Count == 0) return 0f;

        int totalPower = 0;
        foreach (var stats in activeAllies)
        {
            totalPower += stats.level * 10 + Mathf.RoundToInt(stats.currentHP / 10) + Mathf.RoundToInt(stats.currentDMG);
        }

        // Power kỳ vọng tại mỗi level
        float expectedPower = playerLevel * 50f; // Giả sử mỗi level tăng 50 power
        float ratio = totalPower / Mathf.Max(1f, expectedPower);

        return ratio; // <0.7 = yếu, 0.7-1.3 = bình thường, >1.3 = mạnh
    }

    [ContextMenu("Debug Player State")]
    public void DebugPlayerState()
    {
        Debug.Log($"=== PLAYER STATE ===\n" +
                  $"Player Level: {playerLevel}\n" +
                  $"Level Tier: {GetLevelTier(playerLevel)}\n" +
                  $"Active Allies: {activeAllies.Count}\n" +
                  $"Power Ratio: {GetPowerToLevelRatio():F2}\n" +
                  $"Recommended Enemy Level: {GetRecommendedEnemyLevel()}\n" +
                  $"Current State: {GetSimpleState()}");
    }
}