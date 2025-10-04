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
        // chỉ lấy collider trong enemyLayer
        Collider2D[] hits = Physics2D.OverlapCircleAll(_self.position, _detectionRadius, _enemyLayer);

        Transform nearestEnemy = hits
            .OrderBy(h => Vector2.Distance(_self.position, h.transform.position))
            .Select(h => h.transform)
            .FirstOrDefault();

        if (nearestEnemy != null)
        {
            SetData("target", nearestEnemy); // lưu vào blackboard
            state = NodeState.SUCCESS;
            return state;
        }

        ClearData("target");
        state = NodeState.FAILURE;
        return state;
    }
}
