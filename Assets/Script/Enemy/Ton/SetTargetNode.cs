using BehaviorTree;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TonSetTargetNode : Nodes
{
    private TonMovement _tonMovement;

    public TonSetTargetNode(TonMovement tonMovement)
    {
        _tonMovement = tonMovement;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        // Kiểm tra target còn hợp lệ hay không
        if (target != null
            && target.gameObject.activeInHierarchy)
        {
            Stats stats = target.GetComponent<Stats>();
            if (stats != null && stats.currentHP > 0)
            {
                if (_tonMovement.getTarget() != target)
                    _tonMovement.SetTarget(target);
                state = NodeState.SUCCESS;
            }
            else
            {
                // Target đã chết
                state = NodeState.FAILURE;
            }
        }
        else
        {
            // Target không tồn tại hoặc bị ẩn
            state = NodeState.FAILURE;
        }

        return state;
    }
}
