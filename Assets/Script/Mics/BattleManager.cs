using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public BattleStateCollector stateCollector;
    public QLearningEnemyBalancer qLearning;

    private int chosenDifficulty;

    void Start()
    {
        string state = stateCollector.currentState;
        chosenDifficulty = qLearning.ChooseEnemySetup(state);

        EnemySpawner.SpawnEnemies(chosenDifficulty);
        Debug.Log($"Spawned enemies with difficulty: {chosenDifficulty}");
    }

    public void OnBattleEnd(bool allyWon, float duration)
    {
        float reward = EvaluateReward(allyWon, duration);
        string nextState = stateCollector.currentState;

        qLearning.UpdateAfterBattle(nextState, reward);
        qLearning.DebugQTable();
    }

    float EvaluateReward(bool allyWon, float duration)
    {
        if (allyWon && duration < 10f) return -10f; // quá dễ
        if (allyWon && duration < 25f) return +10f; // cân bằng
        if (!allyWon && duration > 20f) return +5f; // thua sát nút
        return -5f; // quá chênh lệch
    }
}
