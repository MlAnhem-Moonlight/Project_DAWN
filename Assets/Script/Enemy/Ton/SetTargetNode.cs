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
        if (target != null)
        {
            if (_tonMovement.getTarget() != target) _tonMovement.SetTarget(target);
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.FAILURE;
        }
        
        return state;
    }
}
