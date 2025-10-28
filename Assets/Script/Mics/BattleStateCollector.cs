using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gom thông tin từ toàn bộ ally đang tồn tại trên sân
/// để tạo state cho Q-Learning.
/// </summary>
public class BattleStateCollector : MonoBehaviour
{
    [Header("Runtime")]
    [Tooltip("Chuỗi mô tả đội ally hiện tại, ví dụ: Archer-Lv3-HP80-DMG15|Mage-Lv2-HP60-DMG25")]
    public string currentState = "";

    [Tooltip("Danh sách ally hiện đang tồn tại trên sân")]
    public List<Stats> activeAllies = new List<Stats>();

    void Update()
    {
        RefreshActiveAllies();
        currentState = BuildStateString();
    }

    /// <summary>
    /// Cập nhật danh sách ally đang tồn tại trên sân.
    /// </summary>
    void RefreshActiveAllies()
    {
        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag("Ally");
        activeAllies = allyObjects
            .Select(o => o.GetComponent<Stats>())
            .Where(s => s != null && s.gameObject.activeInHierarchy)
            .ToList();
    }

    /// <summary>
    /// Tạo chuỗi state từ các chỉ số cơ bản của ally (Level, HP, Damage).
    /// </summary>
    string BuildStateString()
    {
        List<string> summaries = new List<string>();

        foreach (var stats in activeAllies)
        {
            string type = stats.baseStats != null ? stats.baseStats.name : stats.name;
            int hp = Mathf.RoundToInt(stats.currentHP);
            int dmg = Mathf.RoundToInt(stats.currentDMG);
            summaries.Add($"{type}-Lv{stats.level}-HP{hp}-DMG{dmg}");
        }

        summaries.Sort(); // đảm bảo thứ tự ổn định
        return string.Join("|", summaries);
    }
}
