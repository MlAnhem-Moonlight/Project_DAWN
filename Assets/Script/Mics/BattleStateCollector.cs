using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Thu thập state từ đội hình ally và tạo state đơn giản hóa cho Q-Learning
/// </summary>
public class BattleStateCollector : MonoBehaviour
{
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
            return "Empty";

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

        // State format: "Composition_Count_AvgLevel_HPCategory_DMGCategory"
        string state = $"{composition}_{count}u_L{totalLevel / count}_H{hpCategory}_D{dmgCategory}";

        return state;
    }

    /// <summary>
    /// State đơn giản hơn nữa - chỉ dùng composition và power tier
    /// </summary>
    public string GetSimpleState()
    {
        if (activeAllies.Count == 0)
            return "Empty";

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

        // Phân power thành tiers (Weak: 0-100, Medium: 101-200, Strong: 201+)
        string powerTier = totalPower <= 100 ? "Weak" :
                          totalPower <= 200 ? "Medium" :
                          totalPower <= 350 ? "Strong" : "VeryStrong";

        // Composition
        var sortedUnits = unitCounts.OrderBy(kvp => kvp.Key).ToList();
        string composition = string.Join("_", sortedUnits.Select(kvp => $"{kvp.Key}x{kvp.Value}"));

        return $"{composition}_{powerTier}";
    }
}