using BehaviorTree;
using UnityEngine;

public class ArcherDef : Nodes
{
    private Transform _transform;

    public ArcherDef(Transform transform)
    {
        _transform = transform;
    }
    public override NodeState Evaluate()
    {
        if (_transform.GetComponent<ArcherBehavior>().spearState != AllyState.Defensive)
        {

            state = NodeState.FAILURE;
            return state;
        }

        return state;
    }
}
