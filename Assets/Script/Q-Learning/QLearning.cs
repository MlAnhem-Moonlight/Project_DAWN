using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Q-Learning logic (độc lập với gameplay).
/// </summary>
public class QLearning : MonoBehaviour
{
    [Header("Q-Learning Parameters")]
    public float learningRate = 0.1f;     // α
    public float discountFactor = 0.9f;   // γ
    public float explorationRate = 0.2f;  // ε
    public int maxActions = 4;            // số lượng hành động có thể chọn

    // Q-table: key = state, value = list Q-values cho từng action
    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();

    /// <summary>
    /// Chọn hành động dựa trên Q-table + epsilon-greedy.
    /// </summary>
    public int ChooseAction(string state)
    {
        if (!qTable.ContainsKey(state))
            qTable[state] = new float[maxActions];

        // random exploration
        if (Random.value < explorationRate)
            return Random.Range(0, maxActions);

        // exploit: chọn hành động có Q cao nhất
        float[] actions = qTable[state];
        int bestAction = 0;
        float bestValue = actions[0];
        for (int i = 1; i < actions.Length; i++)
        {
            if (actions[i] > bestValue)
            {
                bestValue = actions[i];
                bestAction = i;
            }
        }
        return bestAction;
    }

    /// <summary>
    /// Cập nhật giá trị Q.
    /// </summary>
    public void UpdateQ(string state, int action, float reward, string nextState)
    {
        if (!qTable.ContainsKey(state))
            qTable[state] = new float[maxActions];
        if (!qTable.ContainsKey(nextState))
            qTable[nextState] = new float[maxActions];

        float oldQ = qTable[state][action];
        float maxNextQ = Mathf.Max(qTable[nextState]);
        float newQ = oldQ + learningRate * (reward + discountFactor * maxNextQ - oldQ);
        qTable[state][action] = newQ;
    }

    /// <summary>
    /// In ra 1 phần của Q-table để debug.
    /// </summary>
    public void DebugQTable()
    {
        foreach (var kvp in qTable)
        {
            Debug.Log($"State: {kvp.Key} => {string.Join(", ", kvp.Value)}");
        }
    }
}
