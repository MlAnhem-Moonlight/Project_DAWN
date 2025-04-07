using BehaviorTree;
using UnityEngine;

public class TheHandSetTarget : Nodes
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TheHandMovement _theHandMovement;

    public TheHandSetTarget(TheHandMovement theHandMovement)
    {
        _theHandMovement = theHandMovement;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target != null)
        {
            _theHandMovement.SetTarget(target);
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.FAILURE;
        }

        return state;
    }
}
