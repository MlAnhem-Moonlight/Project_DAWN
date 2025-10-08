using BehaviorTree;
using UnityEngine;
using System.Linq;

public class CheckEnemyInRangeAlly : Nodes
{
    private Transform _self;
    private float _detectionRadius;
    private LayerMask _enemyLayer;

    public CheckEnemyInRangeAlly(Transform self, float detectionRadius, LayerMask enemyLayer)
    {
        _self = self;
        _detectionRadius = detectionRadius;
        _enemyLayer = enemyLayer;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_self.position, _detectionRadius, _enemyLayer);
        
        Transform nearestEnemy = hits
            .Where(h => h.GetComponent<Stats>() != null) // chỉ lấy object có script Stats
            .OrderBy(h => Vector2.Distance(_self.position, h.transform.position))
            .Select(h => h.transform)
            .FirstOrDefault();

        if (nearestEnemy != null)
        {
            parent.SetData("target", nearestEnemy); // lưu vào blackboard
            parent?.parent?.SetData("target", nearestEnemy); // lưu vào blackboard của root (nếu có)
            state = NodeState.SUCCESS;
            return state;
        }

        parent.ClearData("target");
        parent?.parent ?.ClearData("target");
        _self.GetComponent<SpearBehavior>().currentState = AnimatorState.Idle;
        state = NodeState.FAILURE;
        return state;
    }


}
