using System.Collections.Generic;
using UnityEngine;

public class QLearningEnemyBalancer : MonoBehaviour
{
    [Header("Q-Learning Parameters")]
    public float learningRate = 0.1f;
    public float discountFactor = 0.9f;
    public float explorationRate = 0.2f;
    public int difficultyLevels = 4;      // 0 = yếu → 3 = cực mạnh

    private Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
    private string lastState;
    private int lastAction;

    public int ChooseEnemySetup(string state)
    {
        if (!qTable.ContainsKey(state))
            qTable[state] = new float[difficultyLevels];

        // ε-greedy
        if (Random.value < explorationRate)
        {
            lastAction = Random.Range(0, difficultyLevels);
        }
        else
        {
            float[] qValues = qTable[state];
            lastAction = System.Array.IndexOf(qValues, Mathf.Max(qValues));
        }

        lastState = state;
        return lastAction;
    }

    public void UpdateAfterBattle(string nextState, float reward)
    {
        if (string.IsNullOrEmpty(lastState)) return;

        if (!qTable.ContainsKey(lastState))
            qTable[lastState] = new float[difficultyLevels];
        if (!qTable.ContainsKey(nextState))
            qTable[nextState] = new float[difficultyLevels];

        float oldQ = qTable[lastState][lastAction];
        float maxNextQ = Mathf.Max(qTable[nextState]);
        float newQ = oldQ + learningRate * (reward + discountFactor * maxNextQ - oldQ);

        qTable[lastState][lastAction] = newQ;
    }

    public void DebugQTable()
    {
        foreach (var kvp in qTable)
            Debug.Log($"State: {kvp.Key} => [{string.Join(", ", kvp.Value)}]");
    }
}
