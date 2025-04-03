using BehaviorTree;
using UnityEngine;

public class MageSetTargetNode : Nodes
{
    private TheMageMovement _theMageMovement;

    public MageSetTargetNode(TheMageMovement theMageMovement)
    {
        _theMageMovement = theMageMovement;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target != null)
        {
            _theMageMovement.SetTarget(target);
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.FAILURE;
        }

        return state;
    }
}
