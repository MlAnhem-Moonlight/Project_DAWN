using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gom thông tin từ tất cả unit ally đang tồn tại trên sân
/// để tạo state cho Q-Learning.
/// </summary>
public class BattleStateCollector : MonoBehaviour
{
    [Header("Q-Learning Reference")]
    public QLearning qLearning;

    [Header("Runtime")]
    [Tooltip("State hiện tại dưới dạng chuỗi, ví dụ: A3_HP80_SPD5|M2_HP100_SPD3")]
    public string currentState = "";

    [Tooltip("Danh sách ally đang tồn tại")]
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
    /// Xây dựng chuỗi state từ thông tin của các ally.
    /// </summary>
    string BuildStateString()
    {
        List<string> allySummaries = new List<string>();

        foreach (var stats in activeAllies)
        {
            string type = stats.baseStats != null ? stats.baseStats.name : stats.name;
            string info = $"{type}-Lv{stats.level}-HP{Mathf.RoundToInt(stats.currentHP)}-DMG{Mathf.RoundToInt(stats.currentDMG)}";
            allySummaries.Add(info);
        }

        allySummaries.Sort(); // sắp xếp cho ổn định state
        return string.Join("|", allySummaries);
    }

    /// <summary>
    /// Gọi Q-learning để chọn hành động dựa trên state hiện tại.
    /// </summary>
    public int ChooseAction()
    {
        if (qLearning == null)
        {
            Debug.LogWarning("QLearning reference missing!");
            return 0;
        }

        return qLearning.ChooseAction(currentState);
    }

    /// <summary>
    /// Cập nhật phần thưởng cho Q-learning (ví dụ sau khi thắng / thua).
    /// </summary>
    public void GiveReward(float reward, string nextState)
    {
        if (qLearning == null)
        {
            Debug.LogWarning("QLearning reference missing!");
            return;
        }

        int lastAction = ChooseAction();
        qLearning.UpdateQ(currentState, lastAction, reward, nextState);
    }
}
