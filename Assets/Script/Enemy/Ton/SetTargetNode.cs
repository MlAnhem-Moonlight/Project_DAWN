using BehaviorTree;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetTargetNode : Nodes
{
    private TonMovement _tonMovement;

    public SetTargetNode(TonMovement tonMovement)
    {
        _tonMovement = tonMovement;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target != null)
        {
            _tonMovement.SetTarget(target);
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.FAILURE;
        }

        return state;
    }
}
